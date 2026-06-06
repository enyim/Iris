using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class PageGetResourceTreeHandler : ICdpCommandHandler<Page.GetResourceTreeRequest, Page.GetResourceTreeRequestResult>
{
	public ValueTask<Page.GetResourceTreeRequestResult> HandleAsync(Page.GetResourceTreeRequest parameters, CdpCommandContext context)
	{
		var opts = context.Services.GetRequiredService<InspectionTargetOptions>();
		return new(new Page.GetResourceTreeRequestResult(
			new Page.FrameResourceTreeType(InspectionHelpers.MakeFrame(opts.Url), Resources: [])));
	}
}
