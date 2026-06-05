namespace Enyim.Iris.Server.Targets;

/// <summary>
/// Holds the set of debuggable targets surfaced by the discovery endpoints. The default
/// implementation is in-memory; replace it to back targets by real application state.
/// </summary>
public interface ICdpTargetRegistry
{
	/// <summary>The id of the browser-level endpoint (<c>/devtools/browser/{id}</c>).</summary>
	string BrowserId { get; }

	IReadOnlyCollection<CdpTarget> GetTargets();

	bool TryGet(string id, out CdpTarget target);

	/// <summary>Adds or replaces a target.</summary>
	CdpTarget Add(CdpTarget target);

	bool Remove(string id);

	/// <summary>Convenience for <c>/json/new</c>: creates and registers a new page target.</summary>
	CdpTarget CreatePage(string url);
}
