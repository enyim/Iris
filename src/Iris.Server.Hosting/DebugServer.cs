using Enyim.Iris.Server.AspNetCore;
using Enyim.Iris.Server.Targets;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enyim.Iris.Server.Hosting;

/// <summary>
/// Factory and implementation for an embedded debug server. The server owns an internal Kestrel
/// listener on a loopback port. Use <see cref="Create"/> to configure it, then call
/// <see cref="StartAsync"/> to begin listening.
/// </summary>
public sealed class DebugServer : IDebugServer
{
	private readonly WebApplication _app;

	private DebugServer(WebApplication app, DebugServerOptions options)
	{
		_app = app;
		InspectUrl = new Uri($"http://127.0.0.1:{options.Port}/json/list");
	}

	/// <inheritdoc/>
	public Uri InspectUrl { get; }

	/// <inheritdoc/>
	public IServiceProvider Services => _app.Services;

	/// <summary>Creates and configures an embedded debug server. Call <see cref="StartAsync"/> to begin listening.</summary>
	/// <param name="configure">Options for the server (port, target info, browser identity).</param>
	/// <param name="configureCdp">Optional callback to register CDP command handlers on the builder.</param>
	public static IDebugServer Create(Action<DebugServerOptions> configure,
	                                  Action<ICdpServerBuilder>? configureCdp = null)
	{
		var options = new DebugServerOptions();
		configure(options);

		var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
		{
			EnvironmentName = "Production",
		});
		// Configure the loopback-only listener via IConfiguration (works with slim builder).
		builder.Configuration["Kestrel:Endpoints:Http:Url"] = $"http://127.0.0.1:{options.Port}";

		var cdpBuilder = builder.Services.AddCdpServer(o =>
		{
			o.BrowserName = options.BrowserName;
			o.ProtocolVersion = options.ProtocolVersion;
			o.UserAgent = options.UserAgent;
		});

		configureCdp?.Invoke(cdpBuilder);

		var app = builder.Build();

		var registry = app.Services.GetRequiredService<ICdpTargetRegistry>();
		registry.Add(new CdpTarget
		{
			Id = "95b6f5dc-ce10-412a-82ea-a18d9a1dcb94",
			Type = "page",
			Title = options.TargetTitle,
			Url = options.TargetUrl,
		});

		var httpLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Iris.Http");
		app.Use(async (ctx, next) =>
		{
			if (ctx.WebSockets.IsWebSocketRequest)
			{
				httpLogger.LogInformation("WS {Path} -> open", ctx.Request.Path);
				await next(ctx);
				httpLogger.LogInformation("WS {Path} -> closed", ctx.Request.Path);
			}
			else
			{
				await next(ctx);
				httpLogger.LogInformation("{Method} {Path} -> {Status}", ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode);
			}
		});

		app.UseWebSockets();
		app.MapCdpServer();

		return new DebugServer(app, options);
	}

	/// <inheritdoc/>
	public Task StartAsync(CancellationToken ct = default) => _app.StartAsync(ct);

	/// <inheritdoc/>
	public Task StopAsync(CancellationToken ct = default) => _app.StopAsync(ct);

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await _app.DisposeAsync().ConfigureAwait(false);
	}
}
