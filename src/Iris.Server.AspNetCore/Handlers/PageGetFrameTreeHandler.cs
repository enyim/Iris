using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class PageGetFrameTreeHandler : ICdpCommandHandler<Page.GetFrameTreeRequest, Page.GetFrameTreeRequestResult>
{
	public ValueTask<Page.GetFrameTreeRequestResult> HandleAsync(Page.GetFrameTreeRequest parameters, CdpCommandContext context)
	{
		var opts = context.Services.GetRequiredService<InspectionTargetOptions>();
		return new(new Page.GetFrameTreeRequestResult(new Page.FrameTreeType(InspectionHelpers.MakeFrame(opts.Url))));
	}
}
