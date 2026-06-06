using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomGetDocumentHandler : ICdpCommandHandler<DOM.GetDocumentRequest, DOM.GetDocumentRequestResult>
{
	public ValueTask<DOM.GetDocumentRequestResult> HandleAsync(DOM.GetDocumentRequest parameters, CdpCommandContext context)
	{
		var opts = context.Services.GetRequiredService<InspectionTargetOptions>();
		var store = context.Services.GetRequiredService<IInspectionSnapshotStore>();
		var mapper = context.Services.GetRequiredService<DebugNodeMapper>();
		var tree = store.CurrentTree;

		if (tree is null)
			return new(new DOM.GetDocumentRequestResult(InspectionHelpers.EmptyDocument(opts.Url)));

		return new(new DOM.GetDocumentRequestResult(mapper.MapTree(tree)));
	}
}
