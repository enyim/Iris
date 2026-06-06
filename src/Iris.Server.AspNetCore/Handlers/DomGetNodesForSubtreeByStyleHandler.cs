using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomGetNodesForSubtreeByStyleHandler(IInspectionSnapshotStore store)
	: ICdpCommandHandler<DOM.GetNodesForSubtreeByStyleRequest, DOM.GetNodesForSubtreeByStyleRequestResult>
{
	public ValueTask<DOM.GetNodesForSubtreeByStyleRequestResult> HandleAsync(DOM.GetNodesForSubtreeByStyleRequest parameters, CdpCommandContext context)
	{
		var tree = store.CurrentTree;

		if (tree is null)
			return new(new DOM.GetNodesForSubtreeByStyleRequestResult([]));

		return new(new DOM.GetNodesForSubtreeByStyleRequestResult([]));
	}
}
