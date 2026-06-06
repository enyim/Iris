using Enyim.Iris.Protocol;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;
using Enyim.Iris.Server.Protocol;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomGetBoxModelHandler(IInspectionSnapshotStore store) : ICdpCommandHandler<DOM.GetBoxModelRequest, DOM.GetBoxModelRequestResult>
{
	public ValueTask<DOM.GetBoxModelRequestResult> HandleAsync(DOM.GetBoxModelRequest parameters, CdpCommandContext context)
	{
		var nodeId = parameters.NodeId.RequiredValue();
		var node = store.GetNodeById(nodeId);

		if (node.BoxModel is not { } bm)
			return new(new DOM.GetBoxModelRequestResult(new DOM.BoxModelType(Content: Zero, Padding: Zero, Border: Zero, Margin: Zero, Width: 0, Height: 0)));

		return new(new DOM.GetBoxModelRequestResult(new DOM.BoxModelType(
			Content: ToQuad(bm.Content),
			Padding: ToQuad(bm.Padding),
			Border:  ToQuad(bm.Border),
			Margin:  ToQuad(bm.Margin),
			Width:   bm.Border.X2 - bm.Border.X1,
			Height:  bm.Border.Y4 - bm.Border.Y1)));

		static DOM.QuadType ToQuad(Quad q) => new([q.X1, q.Y1, q.X2, q.Y2, q.X3, q.Y3, q.X4, q.Y4]);
	}

	private static readonly DOM.QuadType Zero = new([0, 0, 0, 0, 0, 0, 0, 0]);
}
