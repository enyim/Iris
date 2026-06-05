using Enyim.Iris.Server.Inspection;

namespace Enyim.Iris.Server.Hosting;

/// <summary>
/// Embedded debug server lifecycle and push API. Obtain via <see cref="DebugServer.Create"/>.
/// </summary>
public interface IDebugServer : IAsyncDisposable
{
	/// <summary>The HTTP base URL for the CDP discovery endpoint (<c>/json/list</c>).</summary>
	Uri InspectUrl { get; }

	Task StartAsync(CancellationToken ct = default);
	Task StopAsync(CancellationToken ct = default);

	/// <summary>
	/// Replaces the cached control-tree snapshot and nudges connected inspectors with
	/// <c>DOM.documentUpdated</c>. Non-blocking: enqueues via <c>TryWrite</c>.
	/// </summary>
	void PublishTree(DebugNode root);

	/// <summary>
	/// Broadcasts a log entry as <c>Log.entryAdded</c> to every session that has enabled the
	/// Log domain. Non-blocking: enqueues via <c>TryWrite</c>.
	/// </summary>
	void Log(DebugLogEntry entry);
}
