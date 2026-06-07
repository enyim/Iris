using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.Handlers;

internal sealed class NetworkEnableHandler : ICdpCommandHandler<Network.EnableRequest, Network.EnableRequestResult>
{
	public ValueTask<Network.EnableRequestResult> HandleAsync(Network.EnableRequest parameters, CdpCommandContext context) =>
		new(new Network.EnableRequestResult());
}
