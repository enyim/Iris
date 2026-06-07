using Enyim.Iris.Protocol;
using Enyim.Iris.Server;
using Enyim.Iris.Server.Dispatch;

using Sample.Inspection;

namespace Sample;

internal sealed class DomGetDocumentHandler(InspectionTargetOptions opts, IInspectionSnapshotStore store, DebugNodeMapper mapper)
	: ICdpCommandHandler<DOM.GetDocumentRequest, DOM.GetDocumentRequestResult>
{
	public ValueTask<DOM.GetDocumentRequestResult> HandleAsync(DOM.GetDocumentRequest parameters, CdpCommandContext context)
	{
		var tree = store.CurrentTree;

		if (tree is null)
			return new(new DOM.GetDocumentRequestResult(EmptyDocument(opts.Url)));

		return new(new DOM.GetDocumentRequestResult(mapper.MapTree(tree)));
	}

	private static DOM.NodeType EmptyDocument(string url) =>
		new(
			NodeId: new DOM.NodeIdType(1),
			BackendNodeId: new DOM.BackendNodeIdType(1),
			NodeTypeProperty: 9,
			NodeName: "#document",
			LocalName: "",
			NodeValue: "",
			DocumentURL: url,
			BaseURL: url,
			ChildNodeCount: 0);
}
