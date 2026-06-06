using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class TrackComputedStyleUpdatesForNodeHandler : ICdpCommandHandler<TrackComputedStyleUpdatesForNodeRequest, TrackComputedStyleUpdatesForNodeRequestResult>
{
	public ValueTask<TrackComputedStyleUpdatesForNodeRequestResult> HandleAsync(TrackComputedStyleUpdatesForNodeRequest parameters, CdpCommandContext context) =>
		new(new TrackComputedStyleUpdatesForNodeRequestResult());
}
