using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Targets;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class TargetGetTargetsHandler : ICdpCommandHandler<Target.GetTargetsRequest, Target.GetTargetsRequestResult>
{
	public ValueTask<Target.GetTargetsRequestResult> HandleAsync(Target.GetTargetsRequest parameters, CdpCommandContext context)
	{
		var registry = context.Services.GetRequiredService<ICdpTargetRegistry>();
		var infos = registry.GetTargets()
			.Select(t => new Target.TargetInfoType(
				TargetId: new Target.TargetIDType(t.Id),
				Type: t.Type,
				Title: t.Title,
				Url: t.Url,
				Attached: true,
				CanAccessOpener: false))
			.ToArray();
		return new(new Target.GetTargetsRequestResult(infos));
	}
}
