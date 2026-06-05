using Enyim.Iris.Server.Inspection;

namespace Enyim.Iris.Server.Hosting;

/// <summary>Configuration for an embedded <see cref="IDebugServer"/>.</summary>
public sealed class DebugServerOptions
{
	/// <summary>Loopback port the embedded Kestrel listener binds to. Default: 9333.</summary>
	public int Port { get; set; } = 9333;

	/// <summary>Title shown in edge://inspect / chrome://inspect for the page target.</summary>
	public string TargetTitle { get; set; } = "Debug Target";

	/// <summary>URL shown in the target list and Page frame tree.</summary>
	public string TargetUrl { get; set; } = "app://main-window";

	/// <summary>
	/// Optional callback invoked on demand when a connected inspector opens the Memory panel.
	/// Return <see langword="null"/> to omit memory data.
	/// </summary>
	public Func<MemoryStats>? MemoryProvider { get; set; }

	// --- CdpServerOptions pass-through ---
	public string BrowserName { get; set; } = "Chrome/124.0.6367.207";
	public string ProtocolVersion { get; set; } = "1.3";
	public string UserAgent { get; set; } =
		"Mozilla/5.0 (compatible; DebugServer/1.0)";
}
