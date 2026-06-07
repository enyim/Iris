using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

namespace Sample;

internal sealed class DomResolveNodeHandler : ICdpCommandHandler<DOM.ResolveNodeRequest, DOM.ResolveNodeRequestResult>
{
	public ValueTask<DOM.ResolveNodeRequestResult> HandleAsync(DOM.ResolveNodeRequest parameters, CdpCommandContext context) =>
		throw new NotImplementedException();
}
