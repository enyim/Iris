namespace Enyim.Iris.Server.Hosting;

/// <summary>
/// Embedded debug server lifecycle and service access. Obtain via <see cref="DebugServer.Create"/>.
/// </summary>
public interface IDebugServer : IAsyncDisposable
{
	/// <summary>The HTTP base URL for the CDP discovery endpoint (<c>/json/list</c>).</summary>
	Uri InspectUrl { get; }

	/// <summary>DI service container for the embedded server. Use to resolve registered services.</summary>
	IServiceProvider Services { get; }

	Task StartAsync(CancellationToken ct = default);
	Task StopAsync(CancellationToken ct = default);
}
