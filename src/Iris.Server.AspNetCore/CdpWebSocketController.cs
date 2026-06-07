using System.Net.WebSockets;

using Enyim.Iris.Server.Sessions;
using Enyim.Iris.Server.Targets;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enyim.Iris.Server.AspNetCore;

[ApiController]
public sealed class CdpWebSocketController(ICdpTargetRegistry registry, CdpSessionFactory sessionFactory, Func<WebSocket, WebSocketCdpConnection> connectionFactory) : ControllerBase
{
	[HttpGet("/devtools/page/{id}")]
	public async Task PageWebSocket(string id)
	{
		if (!registry.TryGet(id, out _))
		{
			HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
			return;
		}

		if (!HttpContext.WebSockets.IsWebSocketRequest)
		{
			HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
			return;
		}

		using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

		var connection = connectionFactory(socket);

		await using var session = sessionFactory.Create(connection, $"page:{id}");
		await session.RunAsync(HttpContext.RequestAborted);
	}

	//[HttpGet("/devtools/browser/{id}")]
	//public async Task BrowserWebSocket(string id) =>
	//	await RunSessionAsync($"browser:{id}");
}
