using ChromeProtocol.Domains;

namespace Enyim.Iris.Server.Inspection;

/// <summary>
/// Maps neutral model types to CDP wire types. Stateless singleton — safe to share.
/// Node IDs are reassigned per <see cref="MapTree"/> call; callers must broadcast
/// <c>DOM.documentUpdated</c> after publishing so DevTools discards its stale id cache.
/// </summary>
public sealed class DebugNodeMapper
{
	public DOM.NodeType MapTree(DebugNode root)
	{
		var nextId = 1;
		return MapNode(root, ref nextId);
	}

	private static DOM.NodeType MapNode(DebugNode node, ref int nextId)
	{
		var id = nextId++;

		var (nodeTypeProperty, nodeName, localName) = node.Kind switch
		{
			DebugNodeKind.Document => (9, "#document", ""),
			DebugNodeKind.Text => (3, "#text", ""),
			_ => (1, node.Name.ToUpperInvariant(), node.Name.ToLowerInvariant()),
		};

		DOM.NodeType[]? children = null;
		if (node.Children is { Count: > 0 } childList)
		{
			children = new DOM.NodeType[childList.Count];
			for (var i = 0; i < childList.Count; i++)
				children[i] = MapNode(childList[i], ref nextId);
		}

		// CDP attributes are a flat [name, value, name, value, …] string array.
		IReadOnlyList<string>? attrs = null;
		if (node.Attributes is { Count: > 0 } attrList)
		{
			var flat = new string[attrList.Count * 2];
			for (var i = 0; i < attrList.Count; i++)
			{
				flat[i * 2] = attrList[i].Key;
				flat[i * 2 + 1] = attrList[i].Value;
			}
			attrs = flat;
		}

		return new DOM.NodeType(
			NodeId: new DOM.NodeIdType(id),
			BackendNodeId: new DOM.BackendNodeIdType(id),
			NodeTypeProperty: nodeTypeProperty,
			NodeName: nodeName,
			LocalName: localName,
			NodeValue: node.Kind == DebugNodeKind.Text ? node.Name : "",
			ChildNodeCount: children?.Length ?? 0,
			Children: children,
			Attributes: attrs);
	}

	public Log.EntryAdded MapLogEntry(DebugLogEntry entry)
	{
		var level = entry.Level switch
		{
			DebugLogLevel.Verbose => "verbose",
			DebugLogLevel.Warning => "warning",
			DebugLogLevel.Error => "error",
			_ => "info",
		};

		var ts = entry.Timestamp.HasValue
			? entry.Timestamp.Value.ToUnixTimeMilliseconds() / 1000.0
			: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

		return new Log.EntryAdded(new Log.LogEntryType(
			Source: "other",
			Level: level,
			Text: entry.Text,
			Timestamp: new Runtime.TimestampType(ts),
			Url: entry.Source));
	}
}
