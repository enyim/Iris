using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomSetInspectedNodeHandler : ICdpCommandHandler<DOM.SetInspectedNodeRequest, DOM.SetInspectedNodeRequestResult>
{
	public ValueTask<DOM.SetInspectedNodeRequestResult> HandleAsync(DOM.SetInspectedNodeRequest parameters, CdpCommandContext context) =>
		new(new DOM.SetInspectedNodeRequestResult());
}
