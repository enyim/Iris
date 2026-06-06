using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class CssGetComputedStyleForNodeHandler : ICdpCommandHandler<CSS.GetComputedStyleForNodeRequest, CSS.GetComputedStyleForNodeRequestResult>
{
	public ValueTask<CSS.GetComputedStyleForNodeRequestResult> HandleAsync(CSS.GetComputedStyleForNodeRequest parameters, CdpCommandContext context)
	{
		var attrs = new[]
		{
			("display", "block"),
			("width", "400px"),
			("height", "200px"),

			("border-left-width", "0px"),
			("border-top-width", "0px"),
			("border-bottom-width", "0px"),
			("border-right-width", "0px"),

			("padding-left", "0px"),
			("padding-top", "0px"),
			("padding-bottom", "0px"),
			("padding-right", "0px"),

			("margin-left", "10px"),
			("margin-top", "10px"),
			("margin-bottom", "10px"),
			("margin-right", "10px"),

			("box-sizing", "content-box"),
			("position", "static"),
		};

		return new(new CSS.GetComputedStyleForNodeRequestResult(
			attrs.Select(a => new CSS.CSSComputedStylePropertyType(a.Item1, a.Item2)).ToArray()));
	}
}
