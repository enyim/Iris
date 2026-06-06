using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class InspectorEnableHandler : ICdpCommandHandler<Inspector.EnableRequest, Inspector.EnableRequestResult>
{
	public ValueTask<Inspector.EnableRequestResult> HandleAsync(Inspector.EnableRequest parameters, CdpCommandContext context) =>
		new(new Inspector.EnableRequestResult());
}
