using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssTrackComputedStyleUpdatesForNodeHandler : ICdpCommandHandler<CSS.TrackComputedStyleUpdatesForNodeRequest, CSS.TrackComputedStyleUpdatesForNodeRequestResult>
{
	public ValueTask<CSS.TrackComputedStyleUpdatesForNodeRequestResult> HandleAsync(CSS.TrackComputedStyleUpdatesForNodeRequest parameters, CdpCommandContext context) =>
		new(new CSS.TrackComputedStyleUpdatesForNodeRequestResult());
}
