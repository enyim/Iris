using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomRequestChildNodesHandler : ICdpCommandHandler<DOM.RequestChildNodesRequest, DOM.RequestChildNodesRequestResult>
{
	public ValueTask<DOM.RequestChildNodesRequestResult> HandleAsync(DOM.RequestChildNodesRequest parameters, CdpCommandContext context) =>
		new(new DOM.RequestChildNodesRequestResult());
}
