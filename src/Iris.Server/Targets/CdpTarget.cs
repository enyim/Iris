namespace Enyim.Iris.Server.Targets;

/// <summary>
/// A debuggable target advertised by the discovery endpoints (one entry in <c>/json/list</c>).
/// The WebSocket URLs are composed by the host from the request authority, so they are not stored here.
/// </summary>
public sealed record CdpTarget
{
	/// <summary>Stable target id; also the WebSocket route segment (<c>/devtools/page/{id}</c>).</summary>
	public required string Id { get; init; }

	/// <summary>CDP target type, e.g. <c>page</c>, <c>browser</c>, <c>iframe</c>, <c>service_worker</c>.</summary>
	public required string Type { get; init; }

	public string Title { get; init; } = "";

	public string Url { get; init; } = "";

	public string Description { get; init; } = "";

	public string? FaviconUrl { get; init; }
}
