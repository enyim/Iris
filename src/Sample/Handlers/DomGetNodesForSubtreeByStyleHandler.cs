using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

using Sample.Inspection;

namespace Sample;

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
