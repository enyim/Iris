using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssGetPlatformFontsForNodeHandler : ICdpCommandHandler<CSS.GetPlatformFontsForNodeRequest, CSS.GetPlatformFontsForNodeRequestResult>
{
	public ValueTask<CSS.GetPlatformFontsForNodeRequestResult> HandleAsync(CSS.GetPlatformFontsForNodeRequest parameters, CdpCommandContext context) =>
		new(new CSS.GetPlatformFontsForNodeRequestResult([]));
}
