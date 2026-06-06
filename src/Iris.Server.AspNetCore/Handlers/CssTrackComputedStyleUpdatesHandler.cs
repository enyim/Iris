using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssTrackComputedStyleUpdatesHandler : ICdpCommandHandler<CSS.TrackComputedStyleUpdatesRequest, CSS.TrackComputedStyleUpdatesRequestResult>
{
	public ValueTask<CSS.TrackComputedStyleUpdatesRequestResult> HandleAsync(CSS.TrackComputedStyleUpdatesRequest parameters, CdpCommandContext context) =>
		new(new CSS.TrackComputedStyleUpdatesRequestResult());
}
