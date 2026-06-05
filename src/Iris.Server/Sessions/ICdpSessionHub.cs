using ChromeProtocol.Core;

namespace Enyim.Iris.Server.Sessions;

/// <summary>
/// Singleton broadcast hub. Sessions register on connect and unregister on disconnect.
/// Callers use <see cref="BroadcastAsync"/> to deliver an event to every live session;
/// per-session domain-enable gating is enforced inside each session's <c>TryEmit</c>.
/// </summary>
public interface ICdpSessionHub
{
	void Register(CdpSession session);
	void Unregister(CdpSession session);

	/// <summary>
	/// Delivers <paramref name="evt"/> to every currently-registered session. Delivery is
	/// best-effort (<c>TryWrite</c>): a full outbound buffer drops the event for that session
	/// rather than blocking the caller.
	/// </summary>
	ValueTask BroadcastAsync(IEvent evt, CancellationToken cancellationToken = default);
}
