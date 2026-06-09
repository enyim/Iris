using Enyim.Iris.Server.AspNetCore;
using Enyim.Iris.Server.Targets;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Enyim.Iris.Server.Hosting;

/// <summary>
/// Factory and implementation for an embedded debug server. The server owns an internal Kestrel
/// listener on a loopback port. Use <see cref="Create"/> to configure it, then call
/// <see cref="StartAsync"/> to begin listening.
/// </summary>
public sealed class DebugServer : IDebugServer
{
	private readonly WebApplication app;

	private DebugServer(WebApplication app, DebugServerOptions options)
	{
		this.app = app;
		InspectUrl = new Uri($"http://127.0.0.1:{options.Port}/json/list");
	}

	/// <inheritdoc/>
	public Uri InspectUrl { get; }

	/// <inheritdoc/>
	public IServiceProvider Services => app.Services;

	/// <inheritdoc/>
	public ICdpTargetRegistry Targets => app.Services.GetRequiredService<ICdpTargetRegistry>();

	/// <summary>Creates and configures an embedded debug server. Call <see cref="StartAsync"/> to begin listening.</summary>
	/// <param name="configure">Options for the server (port, target info, browser identity).</param>
	/// <param name="configureCdp">Optional callback to register CDP command handlers on the builder.</param>
	public static IDebugServer Create(Action<DebugServerOptions> configure, Action<ICdpServerBuilder>? configureCdp = null, string? environment = null)
	{
		var options = new DebugServerOptions();
		configure(options);

		var hostBuilder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
		{
			EnvironmentName = environment ?? Environments.Production,
		});

		hostBuilder.Logging.ClearProviders().AddDebug();

		hostBuilder.WebHost.ConfigureKestrel(kestrel =>
		{
			kestrel.ListenLocalhost(options.Port);
		});

		var cdpBuilder = hostBuilder.Services.AddCdpServer(o =>
		{
			o.BrowserName = options.BrowserName;
			o.ProtocolVersion = options.ProtocolVersion;
			o.UserAgent = options.UserAgent;
			o.V8Version = options.V8Version;
			o.WebKitVersion = options.WebKitVersion;
			o.PageWebSocketPath = options.PageWebSocketPath;
			o.BrowserWebSocketPath = options.BrowserWebSocketPath;
			o.DevToolsFrontendUrlFormat = options.DevToolsFrontendUrlFormat;
		});

		configureCdp?.Invoke(cdpBuilder);

		var app = hostBuilder.Build();

		var registry = app.Services.GetRequiredService<ICdpTargetRegistry>();
		foreach (var target in app.Services.GetServices<CdpTarget>())
			registry.Add(target);

		app.UseWebSockets();
		app.MapCdpServer();

		return new DebugServer(app, options);
	}

	/// <inheritdoc/>
	public Task StartAsync(CancellationToken ct = default) => app.StartAsync(ct);

	/// <inheritdoc/>
	public Task StopAsync(CancellationToken ct = default) => app.StopAsync(ct);

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await app.DisposeAsync().ConfigureAwait(false);
	}
}
