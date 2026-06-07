using Enyim.Iris.Server.Protocol;

namespace Enyim.Iris.Server.Dispatch;

/// <summary>Maps CDP method names to handler delegates. Populated at configuration time.</summary>
public interface ICdpCommandRegistry
{
	/// <summary>Registers (or replaces) the handler for a method.</summary>
	ICdpCommandRegistry Map(string method, CdpCommandDelegate handler);

	/// <summary>
	/// Registers a predicate-based handler invoked when <paramref name="predicate"/> returns
	/// <see langword="true"/> for the method name. Checked in registration order, after all
	/// exact-match handlers but before the <see cref="Fallback"/>.
	/// </summary>
	ICdpCommandRegistry MapWhen(Func<string, bool> predicate, CdpCommandDelegate handler);

	bool TryGet(string method, out CdpCommandDelegate handler);

	IReadOnlyCollection<string> Methods { get; }

	/// <summary>
	/// Optional handler invoked when no method-specific handler is registered. When null, unknown
	/// methods return <see cref="CdpErrorCode.MethodNotFound"/>.
	/// </summary>
	CdpCommandDelegate? Fallback { get; set; }
}
