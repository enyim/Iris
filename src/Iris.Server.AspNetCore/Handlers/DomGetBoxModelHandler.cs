using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class DomGetBoxModelHandler : ICdpCommandHandler<DOM.GetBoxModelRequest, DOM.GetBoxModelRequestResult>
{
	public ValueTask<DOM.GetBoxModelRequestResult> HandleAsync(DOM.GetBoxModelRequest parameters, CdpCommandContext context)
	{
		var (x, y, w, h) = (100, 100, 400, 200);

		return new(new DOM.GetBoxModelRequestResult(new DOM.BoxModelType(
			Content: Quad(x, y, w, h),
			Padding: Quad(x, y, w, h),
			Border: Quad(x, y, w, h),
			Margin: Quad(x - 10, y - 10, w + 20, h + 20),
			Width: (int)w,
			Height: (int)h)));

		static DOM.QuadType Quad(double x, double y, double w, double h) => new([x, y, x + w, y, x + w, y + h, x, y + h]);
	}
}
