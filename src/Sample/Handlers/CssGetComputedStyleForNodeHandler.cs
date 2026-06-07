using Enyim.Iris.Protocol;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Protocol;

using Sample.Inspection;

namespace Sample;

internal sealed class CssGetComputedStyleForNodeHandler(IInspectionSnapshotStore store)
	: ICdpCommandHandler<CSS.GetComputedStyleForNodeRequest, CSS.GetComputedStyleForNodeRequestResult>
{
	public ValueTask<CSS.GetComputedStyleForNodeRequestResult> HandleAsync(CSS.GetComputedStyleForNodeRequest parameters, CdpCommandContext context)
	{
		if (!store.TryGetNodeById(parameters.NodeId.Value, out var node))
			throw new CdpProtocolException(CdpError.NodeNotFound(parameters.NodeId.Value));

		var props = node.ComputedStyle is { Count: > 0 } style
			? style.Select(kvp => new CSS.CSSComputedStylePropertyType(kvp.Key, kvp.Value)).ToArray()
			: [];

		return new(new CSS.GetComputedStyleForNodeRequestResult(props, new CSS.ComputedStyleExtraFieldsType(IsAppearanceBase: false)));
	}
}
