namespace Enyim.Iris.Server;

/// <summary>
/// Server-wide configuration: the browser identity reported by <c>/json/version</c> and the
/// route prefixes for the WebSocket and discovery endpoints.
/// </summary>
public sealed class CdpServerOptions
{
	/// <summary>Reported as <c>Browser</c> in <c>/json/version</c>.</summary>
	public string BrowserName { get; set; } = "DebugServer/1.0";

	/// <summary>Reported as <c>Protocol-Version</c>.</summary>
	public string ProtocolVersion { get; set; } = "1.3";

	public string UserAgent { get; set; } =
		"Mozilla/5.0 (compatible; DebugServer/1.0; +https://github.com/)";

	public string V8Version { get; set; } = "18.0.0.0";

	public string WebKitVersion { get; set; } = "100.0.0.0";

	/// <summary>Route segment for page WebSocket endpoints: <c>/devtools/page/{id}</c>.</summary>
	public string PageWebSocketPath { get; set; } = "/devtools/page";

	/// <summary>Route segment for the browser WebSocket endpoint: <c>/devtools/browser/{id}</c>.</summary>
	public string BrowserWebSocketPath { get; set; } = "/devtools/browser";

	/// <summary>
	/// Optional base for the <c>devtoolsFrontendUrl</c> field. <c>{ws}</c> is replaced by the
	/// host-relative WebSocket URL. When null, the field is built from the hosted DevTools frontend.
	/// </summary>
	public string? DevToolsFrontendUrlFormat { get; set; }
}
