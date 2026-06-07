using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

namespace Sample;

internal sealed class CssGetPlatformFontsForNodeHandler : ICdpCommandHandler<CSS.GetPlatformFontsForNodeRequest, CSS.GetPlatformFontsForNodeRequestResult>
{
	public ValueTask<CSS.GetPlatformFontsForNodeRequestResult> HandleAsync(CSS.GetPlatformFontsForNodeRequest parameters, CdpCommandContext context) =>
		new(new CSS.GetPlatformFontsForNodeRequestResult([]));
}
