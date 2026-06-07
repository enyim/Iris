using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

namespace Sample;

internal sealed class DomRequestChildNodesHandler : ICdpCommandHandler<DOM.RequestChildNodesRequest, DOM.RequestChildNodesRequestResult>
{
	public ValueTask<DOM.RequestChildNodesRequestResult> HandleAsync(DOM.RequestChildNodesRequest parameters, CdpCommandContext context) =>
		new(new DOM.RequestChildNodesRequestResult());
}
