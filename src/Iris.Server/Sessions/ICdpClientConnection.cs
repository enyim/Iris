using Enyim.Iris.Server.Protocol;

namespace Enyim.Iris.Server.Sessions;

/// <summary>
/// A single connected CDP client (one WebSocket). Handlers use it to identify the caller and to
/// enqueue outgoing messages; all writes are serialized by the connection's single writer task.
/// </summary>
public interface ICdpClientConnection
{
	/// <summary>Stable identifier for this connection (for logging/diagnostics).</summary>
	string ConnectionId { get; }

	/// <summary>
	/// The CDP session id this connection represents, or <c>null</c> for the root/browser session.
	/// In v1 there is a single (root) session; this leaves room for flatten/attach routing later.
	/// </summary>
	string? SessionId { get; }

	/// <summary>Queues a message for delivery. Returns once the message is accepted by the outbound queue.</summary>
	ValueTask EnqueueAsync(CdpOutgoingMessage message, CancellationToken cancellationToken = default);
}
