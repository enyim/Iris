using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomResolveNodeHandler : ICdpCommandHandler<DOM.ResolveNodeRequest, DOM.ResolveNodeRequestResult>
{
	public ValueTask<DOM.ResolveNodeRequestResult> HandleAsync(DOM.ResolveNodeRequest parameters, CdpCommandContext context) =>
		throw new NotImplementedException();
}
