using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;

using Microsoft.Extensions.Options;

namespace Enyim.Iris.Server.Handlers;

internal sealed class BrowserGetVersionHandler(IOptions<CdpServerOptions> options) : ICdpCommandHandler<Browser.GetVersionRequest, Browser.GetVersionRequestResult>
{
	public ValueTask<Browser.GetVersionRequestResult> HandleAsync(Browser.GetVersionRequest parameters, CdpCommandContext context) =>
		new(new Browser.GetVersionRequestResult(
			ProtocolVersion: options.Value.ProtocolVersion,
			Product: options.Value.BrowserName,
			Revision: "0",
			UserAgent: options.Value.UserAgent,
			JsVersion: "0.0.0"));
}
