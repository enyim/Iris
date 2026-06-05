using Enyim.Iris.Server.Inspection;

namespace Enyim.Iris.Server.Tests;

public class DebugNodeMapperTests
{
	private readonly DebugNodeMapper _mapper = new();

	[Fact]
	public void MapTree_document_node_has_correct_type()
	{
		var doc = _mapper.MapTree(new DebugNode("d", "#document", DebugNodeKind.Document));
		Assert.Equal(9, doc.NodeTypeProperty);
		Assert.Equal("#document", doc.NodeName);
	}

	[Fact]
	public void MapTree_element_names_are_uppercased()
	{
		var node = _mapper.MapTree(new DebugNode("e", "div"));
		Assert.Equal("DIV", node.NodeName);
		Assert.Equal("div", node.LocalName);
	}

	[Fact]
	public void MapTree_assigns_sequential_ids_depth_first()
	{
		var root = new DebugNode("r", "#document", DebugNodeKind.Document,
			Children: [
				new DebugNode("a", "A"),
				new DebugNode("b", "B"),
			]);

		var mapped = _mapper.MapTree(root);
		Assert.Equal(1, mapped.NodeId.Value);
		Assert.Equal(2, mapped.Children![0].NodeId.Value);
		Assert.Equal(3, mapped.Children![1].NodeId.Value);
	}

	[Fact]
	public void MapTree_attributes_are_flattened_as_name_value_pairs()
	{
		var node = _mapper.MapTree(new DebugNode("n", "SPAN",
			Attributes: [new("id", "x"), new("class", "y")]));

		var attrs = node.Attributes;
		Assert.NotNull(attrs);
		Assert.Equal(["id", "x", "class", "y"], attrs!.ToArray());
	}

	[Fact]
	public void MapTree_text_node_carries_name_as_value()
	{
		var node = _mapper.MapTree(new DebugNode("t", "hello world", DebugNodeKind.Text));
		Assert.Equal(3, node.NodeTypeProperty);
		Assert.Equal("hello world", node.NodeValue);
		Assert.Equal("#text", node.NodeName);
	}

	[Fact]
	public void MapLogEntry_maps_level_correctly()
	{
		var info = _mapper.MapLogEntry(new DebugLogEntry(DebugLogLevel.Info, "msg"));
		var warning = _mapper.MapLogEntry(new DebugLogEntry(DebugLogLevel.Warning, "msg"));
		var error = _mapper.MapLogEntry(new DebugLogEntry(DebugLogLevel.Error, "msg"));
		var verbose = _mapper.MapLogEntry(new DebugLogEntry(DebugLogLevel.Verbose, "msg"));

		Assert.Equal("info", info.Entry.Level);
		Assert.Equal("warning", warning.Entry.Level);
		Assert.Equal("error", error.Entry.Level);
		Assert.Equal("verbose", verbose.Entry.Level);
	}

	[Fact]
	public void MapLogEntry_uses_provided_timestamp()
	{
		var ts = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero);
		var entry = _mapper.MapLogEntry(new DebugLogEntry(DebugLogLevel.Info, "msg", Timestamp: ts));
		var expected = ts.ToUnixTimeMilliseconds() / 1000.0;
		Assert.Equal(expected, entry.Entry.Timestamp.Value, precision: 3);
	}

	[Fact]
	public void SetTree_then_CurrentTree_returns_same_node()
	{
		var store = new InspectionSnapshotStore();
		Assert.Null(store.CurrentTree);

		var node = new DebugNode("r", "ROOT");
		store.SetTree(node);
		Assert.Same(node, store.CurrentTree);
	}
}
