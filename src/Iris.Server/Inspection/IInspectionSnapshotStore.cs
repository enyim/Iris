namespace Enyim.Iris.Server.Inspection;

/// <summary>
/// Thread-safe cache for the host app's latest control-tree snapshot. The app writes via
/// <see cref="SetTree"/> at its own cadence; the server reads <see cref="CurrentTree"/> on
/// every <c>DOM.getDocument</c> request. Slightly stale reads are acceptable by design.
/// </summary>
public interface IInspectionSnapshotStore
{
	void SetTree(DebugNode root);
	DebugNode? CurrentTree { get; }
}

public sealed class InspectionSnapshotStore : IInspectionSnapshotStore
{
	private volatile DebugNode? _tree;

	public void SetTree(DebugNode root) => _tree = root;

	public DebugNode? CurrentTree => _tree;
}
