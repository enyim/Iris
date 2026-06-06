using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class BrowserGetVersionHandler : ICdpCommandHandler<Browser.GetVersionRequest, Browser.GetVersionRequestResult>
{
	public ValueTask<Browser.GetVersionRequestResult> HandleAsync(Browser.GetVersionRequest parameters, CdpCommandContext context) =>
		new(new Browser.GetVersionRequestResult(
			ProtocolVersion: "1.3",
			Product: "DebugServer/1.0",
			Revision: "0",
			UserAgent: "DebugServer/1.0 (Chrome DevTools Protocol)",
			JsVersion: "0.0.0"));
}
