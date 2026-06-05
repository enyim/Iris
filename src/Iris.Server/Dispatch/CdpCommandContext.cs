using System.Text.Json;

using Enyim.Iris.Server.Events;
using Enyim.Iris.Server.Protocol;
using Enyim.Iris.Server.Sessions;

namespace Enyim.Iris.Server.Dispatch;

/// <summary>
/// Everything a command handler needs: the method, raw params, the calling connection, an event
/// emitter, scoped services, and a cancellation token tied to the connection's lifetime.
/// </summary>
public sealed class CdpCommandContext
{
	public required string Method { get; init; }

	/// <summary>Raw params element; <see cref="JsonValueKind.Undefined"/> when none were sent.</summary>
	public required JsonElement Params { get; init; }

	public required ICdpClientConnection Connection { get; init; }

	public required ICdpEventEmitter Events { get; init; }

	/// <summary>Per-command scoped service provider.</summary>
	public required IServiceProvider Services { get; init; }

	public required CancellationToken CancellationToken { get; init; }

	/// <summary>The session id from the incoming message, if any.</summary>
	public string? SessionId { get; init; }

	/// <summary>
	/// Deserializes <see cref="Params"/> into the strongly-typed command record. Treats a missing
	/// params object as an empty one, so parameterless commands work without a body.
	/// </summary>
	public T DeserializeParams<T>()
	{
		if (Params.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
			return JsonSerializer.Deserialize<T>("{}"u8, CdpJson.Payload)!;
		return Params.Deserialize<T>(CdpJson.Payload)!;
	}

	/// <inheritdoc cref="DeserializeParams{T}"/>
	public object? DeserializeParams(Type type)
	{
		if (Params.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
			return JsonSerializer.Deserialize("{}"u8, type, CdpJson.Payload);
		return Params.Deserialize(type, CdpJson.Payload);
	}
}
