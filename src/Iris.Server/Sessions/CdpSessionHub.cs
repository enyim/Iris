using System.Collections.Concurrent;

using ChromeProtocol.Core;

namespace Enyim.Iris.Server.Sessions;

/// <inheritdoc cref="ICdpSessionHub"/>
public sealed class CdpSessionHub : ICdpSessionHub
{
	private readonly ConcurrentDictionary<CdpSession, byte> _sessions = new();

	public void Register(CdpSession session) => _sessions.TryAdd(session, 0);

	public void Unregister(CdpSession session) => _sessions.TryRemove(session, out _);

	public ValueTask BroadcastAsync(IEvent evt, CancellationToken cancellationToken = default)
	{
		foreach (var (session, _) in _sessions)
			session.TryEmit(evt);
		return ValueTask.CompletedTask;
	}
}
