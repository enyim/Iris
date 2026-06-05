using ChromeProtocol.Core;

namespace Enyim.Iris.Server.Events;

/// <summary>
/// Emits CDP events to a client. The event record itself carries both the method name
/// (via <c>[MethodName]</c>) and the event payload (its properties).
/// </summary>
public interface ICdpEventEmitter
{
	/// <summary>
	/// Emits a generated event (e.g. <c>new Runtime.ExecutionContextCreated(...)</c>). Delivery may be
	/// suppressed if the owning domain is not enabled on the target connection.
	/// </summary>
	/// <param name="evt">A generated <see cref="IEvent"/> instance from <c>ChromeProtocol.Domains</c>.</param>
	/// <param name="sessionId">Optional session id override; defaults to the connection's session.</param>
	/// <param name="cancellationToken">Cancels enqueueing the event.</param>
	ValueTask EmitAsync(IEvent evt, string? sessionId = null, CancellationToken cancellationToken = default);
}
