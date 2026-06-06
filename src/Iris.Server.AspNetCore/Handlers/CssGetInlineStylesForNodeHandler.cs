using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssGetInlineStylesForNodeHandler : ICdpCommandHandler<CSS.GetInlineStylesForNodeRequest, CSS.GetInlineStylesForNodeRequestResult>
{
	public ValueTask<CSS.GetInlineStylesForNodeRequestResult> HandleAsync(CSS.GetInlineStylesForNodeRequest parameters, CdpCommandContext context) =>
		new(new CSS.GetInlineStylesForNodeRequestResult());
}
