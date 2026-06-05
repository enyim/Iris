using ChromeProtocol.Core;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Sessions;

namespace Enyim.Iris.Server.Events;

/// <summary>
/// Per-connection event emitter. Resolves the event's method name from its <c>[MethodName]</c>
/// attribute and suppresses events whose domain is gated and not currently enabled.
/// </summary>
public sealed class CdpEventEmitter(
	ICdpClientConnection connection,
	CdpDomainState domainState,
	CdpContractIndex index) : ICdpEventEmitter
{
	public ValueTask EmitAsync(IEvent evt, string? sessionId = null, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(evt);

		var method = index.GetMethodName(evt.GetType());
		var (domain, _) = CdpContractIndex.SplitMethod(method);

		if (index.IsGatedDomain(domain) && !domainState.IsEnabled(domain))
			return ValueTask.CompletedTask;

		var message = new CdpEventMessage(method, evt, sessionId ?? connection.SessionId);
		return connection.EnqueueAsync(message, cancellationToken);
	}
}
