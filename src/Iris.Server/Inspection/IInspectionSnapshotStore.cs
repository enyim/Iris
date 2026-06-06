using System.Collections.Frozen;

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
	private DebugNode? _tree;

	private FrozenDictionary<string, DebugNode> nodesById = new Dictionary<string, DebugNode>().ToFrozenDictionary();

	public void SetTree(DebugNode root)
	{
		_tree = root;
		nodesById = Flatten(root).ToFrozenDictionary();
	}

	public DebugNode? CurrentTree => _tree;

	static IEnumerable<KeyValuePair<string, DebugNode>> Flatten(DebugNode root)
	{
		var stack = new Stack<DebugNode>();
		stack.Push(root);

		while (stack.Count > 0)
		{
			var node = stack.Pop();
			yield return new KeyValuePair<string, DebugNode>(node.Id, node);
			if (node.Children is null) continue;

			foreach (var child in node.Children)
			{
				stack.Push(child);
			}
		}
	}

}
