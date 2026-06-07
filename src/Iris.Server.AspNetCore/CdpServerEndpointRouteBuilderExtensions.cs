using System.Text.Json;

using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Sessions;
using Enyim.Iris.Server.Targets;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>Maps the CDP HTTP discovery endpoints and the WebSocket endpoints.</summary>
public static class CdpServerEndpointRouteBuilderExtensions
{
	public static IEndpointRouteBuilder MapCdpServer(this IEndpointRouteBuilder endpoints)
	{
		var options = endpoints.ServiceProvider.GetRequiredService<IOptions<CdpServerOptions>>().Value;

		endpoints.MapGet("/json/version", GetVersion);
		endpoints.MapGet("/json", ListTargets);
		endpoints.MapGet("/json/list", ListTargets);
		endpoints.MapMethods("/json/new", ["GET", "PUT"], NewTarget);
		endpoints.MapGet("/json/activate/{id}", ActivateTarget);
		endpoints.MapGet("/json/close/{id}", CloseTarget);

		endpoints.Map($"{options.PageWebSocketPath}/{{id}}", PageWebSocket);
		endpoints.Map($"{options.BrowserWebSocketPath}/{{id}}", BrowserWebSocket);

		return endpoints;
	}

	private static Task GetVersion(HttpContext context)
	{
		var options = context.RequestServices.GetRequiredService<IOptions<CdpServerOptions>>().Value;
		var registry = context.RequestServices.GetRequiredService<ICdpTargetRegistry>();
		var wsBase = WebSocketBase(context);

		return WriteJsonAsync(context, new Dictionary<string, object?>
		{
			["Browser"] = options.BrowserName,
			["Protocol-Version"] = options.ProtocolVersion,
			["User-Agent"] = options.UserAgent,
			["V8-Version"] = options.V8Version,
			["WebKit-Version"] = options.WebKitVersion,
			["webSocketDebuggerUrl"] = $"{wsBase}{options.BrowserWebSocketPath}/{registry.BrowserId}",
		});
	}

	private static Task ListTargets(HttpContext context)
	{
		var options = context.RequestServices.GetRequiredService<IOptions<CdpServerOptions>>().Value;
		var registry = context.RequestServices.GetRequiredService<ICdpTargetRegistry>();
		var wsBase = WebSocketBase(context);

		var list = registry.GetTargets()
			.Select(target => DescribeTarget(target, options, wsBase))
			.ToArray();

		return WriteJsonAsync(context, list);
	}

	private static Task NewTarget(HttpContext context)
	{
		var options = context.RequestServices.GetRequiredService<IOptions<CdpServerOptions>>().Value;
		var registry = context.RequestServices.GetRequiredService<ICdpTargetRegistry>();
		var url = context.Request.Query.TryGetValue("url", out var u) ? u.ToString() : "about:blank";

		var target = registry.CreatePage(url);
		return WriteJsonAsync(context, DescribeTarget(target, options, WebSocketBase(context)));
	}

	// Chrome's device-discovery HTTP client is minimal and requires a Content-Length (it does not
	// handle Transfer-Encoding: chunked). Results.Json streams without a length, so write the body
	// ourselves with an explicit Content-Length to stay compatible with chrome://inspect.
	private static async Task WriteJsonAsync(HttpContext context, object? payload)
	{
		var bytes = JsonSerializer.SerializeToUtf8Bytes(payload, CdpJson.Payload);

		context.Response.StatusCode = StatusCodes.Status200OK;
		context.Response.ContentType = "application/json; charset=UTF-8";
		context.Response.ContentLength = bytes.Length;

		await context.Response.Body.WriteAsync(bytes, context.RequestAborted);
	}

	private static IResult ActivateTarget(HttpContext context, string id)
	{
		var registry = context.RequestServices.GetRequiredService<ICdpTargetRegistry>();

		return registry.TryGet(id, out _)
			? Results.Text("Target activated")
			: Results.NotFound($"No such target id: {id}");
	}

	private static IResult CloseTarget(HttpContext context, string id)
	{
		var registry = context.RequestServices.GetRequiredService<ICdpTargetRegistry>();

		return registry.Remove(id)
			? Results.Text("Target is closing")
			: Results.NotFound($"No such target id: {id}");
	}

	private static async Task PageWebSocket(HttpContext context, string id)
	{
		var registry = context.RequestServices.GetRequiredService<ICdpTargetRegistry>();
		if (!registry.TryGet(id, out _))
		{
			context.Response.StatusCode = StatusCodes.Status404NotFound;
			return;
		}

		await RunSession(context, connectionId: $"page:{id}");
	}

	private static async Task BrowserWebSocket(HttpContext context, string id)
	{
		await RunSession(context, connectionId: $"browser:{id}");
	}

	private static async Task RunSession(HttpContext context, string connectionId)
	{
		if (!context.WebSockets.IsWebSocketRequest)
		{
			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			return;
		}

		using var socket = await context.WebSockets.AcceptWebSocketAsync();
		var factory = context.RequestServices.GetRequiredService<CdpSessionFactory>();
		var connection = new WebSocketCdpConnection(socket, context.RequestServices.GetRequiredService<ILogger<WebSocketCdpConnection>>());

		await using var session = factory.Create(connection, connectionId);
		await session.RunAsync(context.RequestAborted);
	}

	private static Dictionary<string, object?> DescribeTarget(
		CdpTarget target, CdpServerOptions options, string wsBase)
	{
		var wsUrl = $"{wsBase}{options.PageWebSocketPath}/{target.Id}";
		var wsHostPath = wsUrl[(wsUrl.IndexOf("://", StringComparison.Ordinal) + 3)..];

		var dto = new Dictionary<string, object?>
		{
			["id"] = target.Id,
			["type"] = target.Type,
			["title"] = target.Title,
			["url"] = target.Url,
			["description"] = target.Description,
			["webSocketDebuggerUrl"] = wsUrl,
			["devtoolsFrontendUrl"] = options.DevToolsFrontendUrlFormat is { } fmt
				? fmt.Replace("{ws}", wsHostPath)
				: $"/devtools/inspector.html?ws={wsHostPath}",
		};

		if (target.FaviconUrl is not null)
			dto["faviconUrl"] = target.FaviconUrl;

		return dto;
	}

	private static string WebSocketBase(HttpContext context)
	{
		var scheme = context.Request.IsHttps ? "wss" : "ws";
		return $"{scheme}://{context.Request.Host.Value}";
	}
}
