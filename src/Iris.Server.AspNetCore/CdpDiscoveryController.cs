using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Targets;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Enyim.Iris.Server.AspNetCore;

[ApiController]
public sealed class CdpDiscoveryController(
	IOptions<CdpServerOptions> options,
	ICdpTargetRegistry registry) : ControllerBase
{
	[HttpGet("/json")]
	[HttpGet("/json/list")]
	public IActionResult ListTargets()
	{
		var wsBase = WsBase();
		var opts = options.Value;
		return CdpContent(registry.GetTargets()
			.Select(t => DescribeTarget(t, opts, wsBase))
			.ToArray());
	}

	[HttpGet("/json/version")]
	public IActionResult GetVersion()
	{
		var opts = options.Value;
		return CdpContent(new CdpVersionInfo
		{
			Browser = opts.BrowserName,
			ProtocolVersion = opts.ProtocolVersion,
			UserAgent = opts.UserAgent,
			V8Version = opts.V8Version,
			WebKitVersion = opts.WebKitVersion,
			WebSocketDebuggerUrl = $"{WsBase()}{opts.BrowserWebSocketPath}/{registry.BrowserId}",
		});
	}

	// TODO bridge these back into the observed app so they can act on it

	[HttpGet("/json/activate/{id}")]
	public IActionResult ActivateTarget(string id) => registry.TryGet(id, out _) ? Ok("Target activated") : NotFound($"No such target id: {id}");

	[HttpGet("/json/close/{id}")]
	public IActionResult CloseTarget(string id) => NotFound($"target {id} cannot be closed");

	private static CdpTargetInfo DescribeTarget(CdpTarget target, CdpServerOptions opts, string wsBase)
	{
		var wsUrl = $"{wsBase}{opts.PageWebSocketPath}/{target.Id}";
		var wsHostPath = wsUrl[(wsUrl.IndexOf("://", StringComparison.Ordinal) + 3)..];

		return new CdpTargetInfo
		{
			Id = target.Id,
			Type = target.Type,
			Title = target.Title,
			Url = target.Url,
			Description = target.Description,
			WebSocketDebuggerUrl = wsUrl,
			DevToolsFrontendUrl = opts.DevToolsFrontendUrlFormat is { } fmt
				? fmt.Replace("{ws}", wsHostPath)
				: $"/devtools/inspector.html?ws={wsHostPath}",
			FaviconUrl = target.FaviconUrl,
		};
	}

	private string WsBase() => $"{(Request.IsHttps ? "wss" : "ws")}://{Request.Host.Value}";

	// Chrome's discovery HTTP client requires Content-Length and does not handle chunked transfer.
	// ContentResult buffers the string and writes the correct byte-length header.
	private ContentResult CdpContent(object value) =>
		Content(JsonSerializer.Serialize(value, CdpJson.Payload), "application/json; charset=UTF-8");
}
