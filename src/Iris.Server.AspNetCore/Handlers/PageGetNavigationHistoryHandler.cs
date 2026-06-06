using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class PageGetNavigationHistoryHandler(InspectionTargetOptions opts)
	: ICdpCommandHandler<Page.GetNavigationHistoryRequest, Page.GetNavigationHistoryRequestResult>
{
	public ValueTask<Page.GetNavigationHistoryRequestResult> HandleAsync(Page.GetNavigationHistoryRequest parameters, CdpCommandContext context) =>
		new(new Page.GetNavigationHistoryRequestResult(
			CurrentIndex: 0,
			Entries: [new Page.NavigationEntryType(
				1, opts.Url, opts.Url, opts.Title,
				new Page.TransitionTypeType("typed"))]));
}
