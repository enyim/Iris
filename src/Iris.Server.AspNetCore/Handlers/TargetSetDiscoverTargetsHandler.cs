using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class TargetSetDiscoverTargetsHandler : ICdpCommandHandler<Target.SetDiscoverTargetsRequest, Target.SetDiscoverTargetsRequestResult>
{
	public ValueTask<Target.SetDiscoverTargetsRequestResult> HandleAsync(Target.SetDiscoverTargetsRequest parameters, CdpCommandContext context) =>
		new(new Target.SetDiscoverTargetsRequestResult());
}
