using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class OverlaySetShowGridOverlaysHandler : ICdpCommandHandler<Overlay.SetShowGridOverlaysRequest, Overlay.SetShowGridOverlaysRequestResult>
{
	public ValueTask<Overlay.SetShowGridOverlaysRequestResult> HandleAsync(Overlay.SetShowGridOverlaysRequest parameters, CdpCommandContext context) =>
		new(new Overlay.SetShowGridOverlaysRequestResult());
}
