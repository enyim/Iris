using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssGetComputedStyleForNodeHandler : ICdpCommandHandler<CSS.GetComputedStyleForNodeRequest, CSS.GetComputedStyleForNodeRequestResult>
{
	public ValueTask<CSS.GetComputedStyleForNodeRequestResult> HandleAsync(CSS.GetComputedStyleForNodeRequest parameters, CdpCommandContext context)
	{
		var store = context.Services.GetRequiredService<IInspectionSnapshotStore>();
		var node = store.GetNodeById(parameters.NodeId.Value);

		var props = node.ComputedStyle is { Count: > 0 } style
			? style.Select(kvp => new CSS.CSSComputedStylePropertyType(kvp.Key, kvp.Value)).ToArray()
			: [];

		return new(new CSS.GetComputedStyleForNodeRequestResult(props));
	}
}
