using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomGetNodesForSubtreeByStyleHandler : ICdpCommandHandler<DOM.GetNodesForSubtreeByStyleRequest, DOM.GetNodesForSubtreeByStyleRequestResult>
{
	public ValueTask<DOM.GetNodesForSubtreeByStyleRequestResult> HandleAsync(DOM.GetNodesForSubtreeByStyleRequest parameters, CdpCommandContext context)
	{
		var store = context.Services.GetRequiredService<IInspectionSnapshotStore>();
		var mapper = context.Services.GetRequiredService<DebugNodeMapper>();
		var tree = store.CurrentTree;

		if (tree is null)
			return new(new DOM.GetNodesForSubtreeByStyleRequestResult([]));

		return new(new DOM.GetNodesForSubtreeByStyleRequestResult([]));
	}
}
