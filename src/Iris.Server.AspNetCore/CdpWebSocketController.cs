using Enyim.Iris.Server.Sessions;
using Enyim.Iris.Server.Targets;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enyim.Iris.Server.AspNetCore;

[ApiController]
public sealed class CdpWebSocketController(
	ICdpTargetRegistry registry,
	ILogger<CdpWebSocketController> logger) : ControllerBase
{
	[HttpGet("/devtools/page/{id}")]
	public async Task PageWebSocket(string id)
	{
		if (!registry.TryGet(id, out _))
		{
			HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
			return;
		}

		await RunSessionAsync($"page:{id}");
	}

	//[HttpGet("/devtools/browser/{id}")]
	//public async Task BrowserWebSocket(string id) =>
	//	await RunSessionAsync($"browser:{id}");

	private async Task RunSessionAsync(string connectionId)
	{
		if (!HttpContext.WebSockets.IsWebSocketRequest)
		{
			HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
			return;
		}

		using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
		var factory = HttpContext.RequestServices.GetRequiredService<CdpSessionFactory>();
		var connection = new WebSocketCdpConnection(socket, logger);

		await using var session = factory.Create(connection, connectionId);
		await session.RunAsync(HttpContext.RequestAborted);
	}
}
