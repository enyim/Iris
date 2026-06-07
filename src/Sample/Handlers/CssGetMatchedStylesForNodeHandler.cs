using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

namespace Sample;

internal sealed class CssGetMatchedStylesForNodeHandler : ICdpCommandHandler<CSS.GetMatchedStylesForNodeRequest, CSS.GetMatchedStylesForNodeRequestResult>
{
	public ValueTask<CSS.GetMatchedStylesForNodeRequestResult> HandleAsync(CSS.GetMatchedStylesForNodeRequest parameters, CdpCommandContext context) =>
		new(new CSS.GetMatchedStylesForNodeRequestResult());
}
