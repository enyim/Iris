using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.Handlers;

internal sealed class LogEnableHandler : ICdpCommandHandler<Log.EnableRequest, Log.EnableRequestResult>
{
	public ValueTask<Log.EnableRequestResult> HandleAsync(Log.EnableRequest parameters, CdpCommandContext context) =>
		new(new Log.EnableRequestResult());
}
