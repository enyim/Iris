using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomPushNodesByBackendIdsToFrontendHandler : ICdpCommandHandler<DOM.PushNodesByBackendIdsToFrontendRequest, DOM.PushNodesByBackendIdsToFrontendRequestResult>
{
	public ValueTask<DOM.PushNodesByBackendIdsToFrontendRequestResult> HandleAsync(DOM.PushNodesByBackendIdsToFrontendRequest parameters, CdpCommandContext context) =>
		new(new DOM.PushNodesByBackendIdsToFrontendRequestResult([]));
}
