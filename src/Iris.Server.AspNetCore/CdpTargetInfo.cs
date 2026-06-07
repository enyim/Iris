using System.Text.Json.Serialization;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>Response body for a single entry in <c>GET /json/list</c> and <c>GET /json/new</c>.</summary>
public sealed record CdpTargetInfo
{
	[JsonPropertyName("id")]
	public required string Id { get; init; }

	[JsonPropertyName("type")]
	public required string Type { get; init; }

	[JsonPropertyName("title")]
	public required string Title { get; init; }

	[JsonPropertyName("url")]
	public required string Url { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("webSocketDebuggerUrl")]
	public required string WebSocketDebuggerUrl { get; init; }

	[JsonPropertyName("devtoolsFrontendUrl")]
	public required string DevToolsFrontendUrl { get; init; }

	[JsonPropertyName("faviconUrl")]
	public string? FaviconUrl { get; init; }
}
