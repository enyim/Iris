using System.Collections.Concurrent;

namespace Enyim.Iris.Server.Sessions;

/// <summary>
/// Tracks which CDP domains are enabled on a single connection. Updated automatically by the
/// session when a <c>Domain.enable</c>/<c>Domain.disable</c> command succeeds, and read by the
/// event emitter to gate events.
/// </summary>
public sealed class CdpDomainState
{
	private readonly ConcurrentDictionary<string, bool> enabled = new(StringComparer.Ordinal);

	public void Enable(string domain) => enabled[domain] = true;

	public void Disable(string domain) => enabled[domain] = false;

	public bool IsEnabled(string domain) => enabled.TryGetValue(domain, out var result) && result;
}
