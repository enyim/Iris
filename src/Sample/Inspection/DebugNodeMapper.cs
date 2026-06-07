using Enyim.Iris.Protocol;

namespace Sample.Inspection;

/// <summary>Maps neutral model types to CDP wire types. Stateless singleton — safe to share.</summary>
public sealed class DebugNodeMapper
{
	public DOM.NodeType MapTree(DebugNode root) => MapNode(root);

	private static DOM.NodeType MapNode(DebugNode node)
	{
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
				children[i] = MapNode(childList[i]);
		}

		// CDP attributes are a flat [name, value, name, value, …] string array.
		IReadOnlyList<string>? attrs = null;
		if (node.Attributes is { Count: > 0 } attrList)
		{
			var flat = new string[attrList.Count * 2];
			var i = 0;
			foreach (var kvp in attrList)
			{
				flat[i * 2] = kvp.Key;
				flat[i * 2 + 1] = kvp.Value;
				i++;
			}
			attrs = flat;
		}

		return new DOM.NodeType(
			NodeId: new DOM.NodeIdType(node.Id),
			BackendNodeId: new DOM.BackendNodeIdType(node.Id),
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
