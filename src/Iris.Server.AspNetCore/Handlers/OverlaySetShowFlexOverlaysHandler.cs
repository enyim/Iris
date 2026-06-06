using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class OverlaySetShowFlexOverlaysHandler : ICdpCommandHandler<Overlay.SetShowFlexOverlaysRequest, Overlay.SetShowFlexOverlaysRequestResult>
{
	public ValueTask<Overlay.SetShowFlexOverlaysRequestResult> HandleAsync(Overlay.SetShowFlexOverlaysRequest parameters, CdpCommandContext context) =>
		new(new Overlay.SetShowFlexOverlaysRequestResult());
}
