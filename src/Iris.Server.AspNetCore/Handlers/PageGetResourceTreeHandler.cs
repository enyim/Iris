using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class PageGetResourceTreeHandler(InspectionTargetOptions opts)
	: ICdpCommandHandler<Page.GetResourceTreeRequest, Page.GetResourceTreeRequestResult>
{
	public ValueTask<Page.GetResourceTreeRequestResult> HandleAsync(Page.GetResourceTreeRequest parameters, CdpCommandContext context) =>
		new(new Page.GetResourceTreeRequestResult(
			new Page.FrameResourceTreeType(InspectionHelpers.MakeFrame(opts.Url), Resources: [])));
}
