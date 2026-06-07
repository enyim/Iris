using System.Buffers;
using System.Net.WebSockets;

using Enyim.Iris.Server.Transport;

using Microsoft.Extensions.Logging;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>Adapts a <see cref="WebSocket"/> to <see cref="ICdpConnection"/>, assembling fragmented text frames.</summary>
public sealed class WebSocketCdpConnection(WebSocket socket, ILogger<WebSocketCdpConnection> logger) : ICdpConnection
{
	private readonly ArrayBufferWriter<byte> receiveBuffer = new(initialCapacity: 8192);

	public async ValueTask<CdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
	{
		receiveBuffer.ResetWrittenCount();

		while (true)
		{
			ValueWebSocketReceiveResult result;
			try
			{
				result = await socket.ReceiveAsync(receiveBuffer.GetMemory(8192), cancellationToken)
					.ConfigureAwait(false);
			}
			catch (WebSocketException)
			{
				return CdpReceiveResult.Closed;
			}

			if (result.MessageType == WebSocketMessageType.Close)
				return CdpReceiveResult.Closed;

			receiveBuffer.Advance(result.Count);

			if (result.EndOfMessage)
				return CdpReceiveResult.Message(receiveBuffer.WrittenMemory);
		}
	}

	public ValueTask SendAsync(ReadOnlyMemory<byte> utf8Payload, CancellationToken cancellationToken)
	{
		return socket.SendAsync(utf8Payload, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
	}

	public async ValueTask CloseAsync(CancellationToken cancellationToken)
	{
		if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
		{
			try
			{
				await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription: null, cancellationToken)
					.ConfigureAwait(false);
			}
			catch (WebSocketException e)
			{
				logger.LogError(e, "Error while closing connection");
			}
		}
	}

	public ValueTask DisposeAsync()
	{
		socket.Dispose();
		return ValueTask.CompletedTask;
	}
}
