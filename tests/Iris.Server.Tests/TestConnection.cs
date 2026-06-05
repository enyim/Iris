using System.Collections.Concurrent;

using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Sessions;

namespace Enyim.Iris.Server.Tests;

/// <summary>An in-memory <see cref="ICdpClientConnection"/> that records every enqueued message.</summary>
internal sealed class TestConnection(string? sessionId = null) : ICdpClientConnection
{
	public ConcurrentQueue<CdpOutgoingMessage> Sent { get; } = new();

	public string ConnectionId { get; } = "test";

	public string? SessionId { get; } = sessionId;

	public ValueTask EnqueueAsync(CdpOutgoingMessage message, CancellationToken cancellationToken = default)
	{
		Sent.Enqueue(message);
		return ValueTask.CompletedTask;
	}
}
