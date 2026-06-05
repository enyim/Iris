using System.Text.Json;

namespace Enyim.Iris.Server.Protocol;

/// <summary>
/// A parsed incoming CDP command. <see cref="Params"/> is a detached (cloned) element, safe to
/// keep after the originating <see cref="JsonDocument"/> is disposed.
/// </summary>
public readonly record struct CdpIncomingMessage(
	long? Id,
	string Method,
	JsonElement Params,
	string? SessionId)
{
	/// <summary>True when the message carried a <c>params</c> object.</summary>
	public bool HasParams => Params.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
}

/// <summary>Result of parsing a raw CDP frame: either a message or an error (with the id if known).</summary>
public readonly record struct CdpParseResult(CdpIncomingMessage Message, CdpError? Error, long? Id)
{
	public bool IsError => Error is not null;

	public static CdpParseResult Ok(CdpIncomingMessage message) => new(message, null, message.Id);
	public static CdpParseResult Fail(CdpError error, long? id = null) => new(default, error, id);
}

/// <summary>Parses raw UTF-8 CDP frames into <see cref="CdpIncomingMessage"/> values.</summary>
public static class CdpWireParser
{
	public static CdpParseResult Parse(ReadOnlyMemory<byte> utf8)
	{
		JsonDocument doc;
		try
		{
			doc = JsonDocument.Parse(utf8);
		}
		catch (JsonException ex)
		{
			return CdpParseResult.Fail(CdpError.ParseError(ex.Message));
		}

		using (doc)
		{
			var root = doc.RootElement;
			if (root.ValueKind != JsonValueKind.Object)
				return CdpParseResult.Fail(CdpError.InvalidRequest("Message must be a JSON object"));

			long? id = null;
			if (root.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number
				&& idEl.TryGetInt64(out var idValue))
			{
				id = idValue;
			}

			if (!root.TryGetProperty("method", out var methodEl)
				|| methodEl.ValueKind != JsonValueKind.String)
			{
				return CdpParseResult.Fail(CdpError.InvalidRequest("Message is missing a 'method' string"), id);
			}

			var method = methodEl.GetString()!;

			var sessionId = root.TryGetProperty("sessionId", out var sEl)
								&& sEl.ValueKind == JsonValueKind.String
				? sEl.GetString()
				: null;

			var @params = root.TryGetProperty("params", out var pEl) ? pEl.Clone() : default;

			return CdpParseResult.Ok(new CdpIncomingMessage(id, method, @params, sessionId));
		}
	}
}
