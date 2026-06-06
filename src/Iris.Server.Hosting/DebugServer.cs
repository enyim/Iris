using Enyim.Iris.Server.AspNetCore;
using Enyim.Iris.Server.Inspection;
using Enyim.Iris.Server.Sessions;
using Enyim.Iris.Server.Targets;

using Enyim.Iris.Protocol;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.Hosting;

/// <summary>
/// Factory and implementation for an embedded debug server. The server owns an internal Kestrel
/// listener on a loopback port and exposes a push API that the host app uses to surface its
/// control tree and log stream to connected DevTools inspectors.
/// </summary>
public sealed class DebugServer : IDebugServer
{
	private readonly WebApplication _app;
	private readonly IInspectionSnapshotStore _store;
	private readonly ICdpSessionHub _hub;
	private readonly DebugNodeMapper _mapper;

	private DebugServer(WebApplication app, DebugServerOptions options)
	{
		_app = app;
		_store = app.Services.GetRequiredService<IInspectionSnapshotStore>();
		_hub = app.Services.GetRequiredService<ICdpSessionHub>();
		_mapper = app.Services.GetRequiredService<DebugNodeMapper>();
		InspectUrl = new Uri($"http://127.0.0.1:{options.Port}/json/list");
	}

	/// <inheritdoc/>
	public Uri InspectUrl { get; }

	/// <summary>Creates and configures an embedded debug server. Call <see cref="StartAsync"/> to begin listening.</summary>
	public static IDebugServer Create(Action<DebugServerOptions> configure)
	{
		var options = new DebugServerOptions();
		configure(options);

		var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
		{
			EnvironmentName = "Production",
		});
		// Configure the loopback-only listener via IConfiguration (works with slim builder).
		builder.Configuration["Kestrel:Endpoints:Http:Url"] = $"http://127.0.0.1:{options.Port}";
		builder.Logging.SetMinimumLevel(LogLevel.Warning);

		builder.Services
			.AddCdpServer(o =>
			{
				o.BrowserName = options.BrowserName;
				o.ProtocolVersion = options.ProtocolVersion;
				o.UserAgent = options.UserAgent;
			})
			.AddInspectionDomains(options.TargetUrl, options.TargetTitle);

		if (options.MemoryProvider is not null)
			builder.Services.AddSingleton(options.MemoryProvider);

		var app = builder.Build();

		var registry = app.Services.GetRequiredService<ICdpTargetRegistry>();
		registry.Add(new CdpTarget
		{
			Id = Guid.NewGuid().ToString("N").ToUpperInvariant(),
			Type = "page",
			Title = options.TargetTitle,
			Url = options.TargetUrl,
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
	public void PublishTree(DebugNode root)
	{
		_store.SetTree(root);
		_hub.BroadcastAsync(new DOM.DocumentUpdated());
	}

	/// <inheritdoc/>
	public void Log(DebugLogEntry entry)
	{
		_hub.BroadcastAsync(_mapper.MapLogEntry(entry));
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await _app.DisposeAsync().ConfigureAwait(false);
	}
}
