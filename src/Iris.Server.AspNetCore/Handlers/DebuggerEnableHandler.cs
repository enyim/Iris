using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DebuggerEnableHandler : ICdpCommandHandler<Debugger.EnableRequest, Debugger.EnableRequestResult>
{
	public ValueTask<Debugger.EnableRequestResult> HandleAsync(Debugger.EnableRequest parameters, CdpCommandContext context) =>
		new(new Debugger.EnableRequestResult(new Runtime.UniqueDebuggerIdType("debugger-1")));
}
