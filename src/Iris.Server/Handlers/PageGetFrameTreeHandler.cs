//using Enyim.Iris.Protocol;

//using Enyim.Iris.Server.Dispatch;

//namespace Enyim.Iris.Server.Handlers;

//internal sealed class PageGetFrameTreeHandler(InspectionTargetOptions opts)
//	: ICdpCommandHandler<Page.GetFrameTreeRequest, Page.GetFrameTreeRequestResult>
//{
//	public ValueTask<Page.GetFrameTreeRequestResult> HandleAsync(Page.GetFrameTreeRequest parameters, CdpCommandContext context) =>
//		new(new Page.GetFrameTreeRequestResult(new Page.FrameTreeType(InspectionHelpers.MakeFrame(opts.Url))));
//}
