using System.Buffers;
using System.Net.WebSockets;

using Enyim.Iris.Server.Transport;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>Adapts a <see cref="WebSocket"/> to <see cref="ICdpConnection"/>, assembling fragmented text frames.</summary>
internal sealed class WebSocketCdpConnection(WebSocket socket) : ICdpConnection
{
	private readonly ArrayBufferWriter<byte> _receiveBuffer = new(initialCapacity: 8192);

	public async ValueTask<CdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
	{
		_receiveBuffer.ResetWrittenCount();

		while (true)
		{
			ValueWebSocketReceiveResult result;
			try
			{
				result = await socket.ReceiveAsync(_receiveBuffer.GetMemory(8192), cancellationToken)
					.ConfigureAwait(false);
			}
			catch (WebSocketException)
			{
				return CdpReceiveResult.Closed;
			}

			if (result.MessageType == WebSocketMessageType.Close)
				return CdpReceiveResult.Closed;

			_receiveBuffer.Advance(result.Count);

			if (result.EndOfMessage)
				return CdpReceiveResult.Message(_receiveBuffer.WrittenMemory);
		}
	}

	public ValueTask SendAsync(ReadOnlyMemory<byte> utf8Payload, CancellationToken cancellationToken) =>
		socket.SendAsync(utf8Payload, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);

	public async ValueTask CloseAsync(CancellationToken cancellationToken)
	{
		if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
		{
			try
			{
				await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, statusDescription: null, cancellationToken)
					.ConfigureAwait(false);
			}
			catch (WebSocketException)
			{
				// Peer already gone; nothing to do.
			}
		}
	}

	public ValueTask DisposeAsync()
	{
		socket.Dispose();
		return ValueTask.CompletedTask;
	}
}
