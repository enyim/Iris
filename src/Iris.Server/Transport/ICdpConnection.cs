namespace Enyim.Iris.Server.Transport;

/// <summary>
/// A bidirectional, message-framed transport for one CDP client (typically a WebSocket).
/// Implementations deliver and accept whole CDP messages as UTF-8 JSON.
/// </summary>
public interface ICdpConnection : IAsyncDisposable
{
	/// <summary>
	/// Receives the next complete message. The returned buffer is owned by the connection and is
	/// only valid until the next <see cref="ReceiveAsync"/> call, so callers must copy anything
	/// they need to retain (the session clones command params before reusing the buffer).
	/// </summary>
	ValueTask<CdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken);

	/// <summary>Sends one complete message. Never called concurrently (the session has a single writer).</summary>
	ValueTask SendAsync(ReadOnlyMemory<byte> utf8Payload, CancellationToken cancellationToken);

	/// <summary>Initiates a graceful close.</summary>
	ValueTask CloseAsync(CancellationToken cancellationToken);
}

/// <summary>The outcome of <see cref="ICdpConnection.ReceiveAsync"/>: a message or a close signal.</summary>
public readonly record struct CdpReceiveResult(ReadOnlyMemory<byte> Payload, bool IsClosed)
{
	public static readonly CdpReceiveResult Closed = new(ReadOnlyMemory<byte>.Empty, true);

	public static CdpReceiveResult Message(ReadOnlyMemory<byte> payload) => new(payload, false);
}
