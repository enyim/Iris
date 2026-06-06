using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class OverlayEnableHandler : ICdpCommandHandler<Overlay.EnableRequest, Overlay.EnableRequestResult>
{
	public ValueTask<Overlay.EnableRequestResult> HandleAsync(Overlay.EnableRequest parameters, CdpCommandContext context) =>
		new(new Overlay.EnableRequestResult());
}
