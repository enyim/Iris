using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssEnableHandler : ICdpCommandHandler<CSS.EnableRequest, CSS.EnableRequestResult>
{
	public ValueTask<CSS.EnableRequestResult> HandleAsync(CSS.EnableRequest parameters, CdpCommandContext context) =>
		new(new CSS.EnableRequestResult());
}
