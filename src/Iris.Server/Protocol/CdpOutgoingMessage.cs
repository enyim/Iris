using System.Buffers;
using System.Text.Json;

namespace Enyim.Iris.Server.Protocol;

/// <summary>
/// A message queued for delivery to a client. Lightweight: the payload object is serialized lazily
/// by the session's single writer task, keeping all socket writes on one thread.
/// </summary>
public abstract record CdpOutgoingMessage
{
	/// <summary>Writes this message as a CDP frame.</summary>
	public abstract void Write(Utf8JsonWriter writer, JsonSerializerOptions options);

	/// <summary>Serializes this message to a UTF-8 byte buffer.</summary>
	public void WriteTo(IBufferWriter<byte> buffer, JsonSerializerOptions options)
	{
		using var writer = new Utf8JsonWriter(buffer);
		Write(writer, options);
	}

	private protected static void WritePayload(
		Utf8JsonWriter writer, string propertyName, object? payload, JsonSerializerOptions options)
	{
		writer.WritePropertyName(propertyName);
		if (payload is null)
		{
			// CDP represents an empty result / params object as {}.
			writer.WriteStartObject();
			writer.WriteEndObject();
		}
		else
		{
			JsonSerializer.Serialize(writer, payload, payload.GetType(), options);
		}
	}
}

/// <summary>A successful command response: <c>{ id, result, sessionId? }</c>.</summary>
public sealed record CdpResultMessage(long Id, object? Result, string? SessionId = null) : CdpOutgoingMessage
{
	public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteNumber("id", Id);
		WritePayload(writer, "result", Result, options);
		if (SessionId is not null) writer.WriteString("sessionId", SessionId);
		writer.WriteEndObject();
	}
}

/// <summary>A failed command response: <c>{ id, error, sessionId? }</c>.</summary>
public sealed record CdpErrorMessage(long Id, CdpError Error, string? SessionId = null) : CdpOutgoingMessage
{
	public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteNumber("id", Id);
		writer.WritePropertyName("error");
		writer.WriteStartObject();
		writer.WriteNumber("code", Error.Code);
		writer.WriteString("message", Error.Message);
		if (Error.Data is not null) writer.WriteString("data", Error.Data);
		writer.WriteEndObject();
		if (SessionId is not null) writer.WriteString("sessionId", SessionId);
		writer.WriteEndObject();
	}
}

/// <summary>An unsolicited event: <c>{ method, params, sessionId? }</c>.</summary>
public sealed record CdpEventMessage(string Method, object? Params, string? SessionId = null) : CdpOutgoingMessage
{
	public override void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString("method", Method);
		WritePayload(writer, "params", Params, options);
		if (SessionId is not null) writer.WriteString("sessionId", SessionId);
		writer.WriteEndObject();
	}
}
