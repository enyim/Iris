using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.Handlers;

internal sealed class DomEnableHandler : ICdpCommandHandler<DOM.EnableRequest, DOM.EnableRequestResult>
{
	public ValueTask<DOM.EnableRequestResult> HandleAsync(DOM.EnableRequest parameters, CdpCommandContext context) =>
		new(new DOM.EnableRequestResult());
}
