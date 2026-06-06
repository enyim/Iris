using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomGetDocumentHandler(InspectionTargetOptions opts, IInspectionSnapshotStore store, DebugNodeMapper mapper)
	: ICdpCommandHandler<DOM.GetDocumentRequest, DOM.GetDocumentRequestResult>
{
	public ValueTask<DOM.GetDocumentRequestResult> HandleAsync(DOM.GetDocumentRequest parameters, CdpCommandContext context)
	{
		var tree = store.CurrentTree;

		if (tree is null)
			return new(new DOM.GetDocumentRequestResult(InspectionHelpers.EmptyDocument(opts.Url)));

		return new(new DOM.GetDocumentRequestResult(mapper.MapTree(tree)));
	}
}
