using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomEnableHandler : ICdpCommandHandler<DOM.EnableRequest, DOM.EnableRequestResult>
{
	public ValueTask<DOM.EnableRequestResult> HandleAsync(DOM.EnableRequest parameters, CdpCommandContext context) =>
		new(new DOM.EnableRequestResult());
}
