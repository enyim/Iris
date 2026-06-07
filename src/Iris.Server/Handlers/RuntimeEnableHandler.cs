using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.Handlers;

internal sealed class RuntimeEnableHandler : ICdpCommandHandler<Runtime.EnableRequest, Runtime.EnableRequestResult>
{
	public async ValueTask<Runtime.EnableRequestResult> HandleAsync(Runtime.EnableRequest parameters, CdpCommandContext context)
	{
		await context.Events.EmitAsync(
			new Runtime.ExecutionContextCreated(
				new Runtime.ExecutionContextDescriptionType(
					Id: new Runtime.ExecutionContextIdType(1),
					Origin: "://",
					Name: "DebugServer",
					UniqueId: "context-1")),
			cancellationToken: context.CancellationToken);
		return new Runtime.EnableRequestResult();
	}
}
