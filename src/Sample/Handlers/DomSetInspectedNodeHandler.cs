using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

namespace Sample;

internal sealed class DomSetInspectedNodeHandler : ICdpCommandHandler<DOM.SetInspectedNodeRequest, DOM.SetInspectedNodeRequestResult>
{
	public ValueTask<DOM.SetInspectedNodeRequestResult> HandleAsync(DOM.SetInspectedNodeRequest parameters, CdpCommandContext context) =>
		new(new DOM.SetInspectedNodeRequestResult());
}
