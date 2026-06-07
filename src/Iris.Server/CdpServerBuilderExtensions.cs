using Enyim.Iris.Server.Handlers;
using Enyim.Iris.Server.Targets;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server;

/// <summary>
/// Registers generic CDP domain handlers for the standard inspection domains (enable/disable
/// acknowledgments, Browser.getVersion, Page, Target) that any inspection server needs.
/// </summary>
public static class CdpServerBuilderExtensions
{
	/// <summary>
	/// Adds generic CDP command handlers for the common inspection protocol handshake
	/// </summary>
	/// <param name="cdp">The server builder returned by <c>AddCdpServer()</c>.</param>
	public static ICdpServerBuilder AddDefaultHandlers(this ICdpServerBuilder cdp)
	{
		// init
		cdp.AddCommandHandler<RuntimeEnableHandler>();
		cdp.AddCommandHandler<DebuggerEnableHandler>();
		cdp.AddCommandHandler<LogEnableHandler>();
		cdp.AddCommandHandler<InspectorEnableHandler>();
		cdp.AddCommandHandler<DomEnableHandler>();
		cdp.AddCommandHandler<CssEnableHandler>();

		cdp.AddCommandHandler<BrowserGetVersionHandler>();

		cdp.AddCommandHandler<TargetGetTargetsHandler>();
		cdp.AddCommandHandler<TargetSetDiscoverTargetsHandler>();

		//cdp.MapWhen(c => c.EndsWith(".enable"), ctx => ValueTask.FromResult(new CdpResult()));

		return cdp;
	}

	/// <summary>Registers a pre-built <see cref="CdpTarget"/> with the server.</summary>
	/// <param name="cdp">The server builder returned by <c>AddCdpServer()</c>.</param>
	/// <param name="target">The target to register.</param>
	public static ICdpServerBuilder AddTarget(this ICdpServerBuilder cdp, CdpTarget target)
	{
		cdp.Services.AddSingleton(target);
		return cdp;
	}

	///// <summary></summary>
	///// <param name="cdp">The server builder returned by <c>AddCdpServer()</c>.</param>
	///// <param name="targetUrl">The URL shown in the target list and Page frame tree.</param>
	///// <param name="targetTitle">The title shown in the target list and navigation history.</param>
	//public static ICdpServerBuilder AddInspectionTarget(this ICdpServerBuilder cdp, string targetUrl = "app://main-window", string targetTitle = "Debug Target")
	//{
	//	//cdp.Services.AddSingleton(new InspectionTargetOptions { Url = targetUrl, Title = targetTitle });

	//	return cdp.AddTarget(new CdpTarget
	//	{
	//		Id = Guid.NewGuid().ToString("D"),
	//		Type = "page",
	//		Title = targetTitle,
	//		Url = targetUrl,
	//	});
	//}
}
