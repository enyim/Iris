using Enyim.Iris.Server.Protocol;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>
/// Registers reusable CDP domain handlers backed by the inspection store and neutral model.
/// Generalises the hand-written sample stubs into a proper library surface that any host can call.
/// </summary>
public static class InspectionDomains
{
	/// <summary>
	/// Adds CDP command handlers for the common inspection domains (Page, DOM, Runtime, Log,
	/// Browser, Target, Debugger, Network) and absorbs optional front-end commands via
	/// <see cref="ICdpServerBuilder.AllowUnhandledCommands"/>.
	/// </summary>
	/// <param name="cdp">The server builder returned by <c>AddCdpServer()</c>.</param>
	/// <param name="targetUrl">The URL shown in the target list and Page frame tree.</param>
	/// <param name="targetTitle">The title shown in the target list and navigation history.</param>
	public static ICdpServerBuilder AddInspectionDomains(this ICdpServerBuilder cdp, string targetUrl = "app://main-window", string targetTitle = "Debug Target")
	{
		cdp.Services.AddSingleton(new InspectionTargetOptions { Url = targetUrl, Title = targetTitle });

		// init
		cdp.AddCommandHandler<RuntimeEnableHandler>();
		cdp.AddCommandHandler<DebuggerEnableHandler>();
		cdp.AddCommandHandler<LogEnableHandler>();
		cdp.AddCommandHandler<InspectorEnableHandler>();
		cdp.AddCommandHandler<OverlayEnableHandler>();

		// Browser
		cdp.AddCommandHandler<BrowserGetVersionHandler>();

		// Target
		//cdp.AddCommandHandler<TargetSetDiscoverTargetsHandler>();
		//cdp.AddCommandHandler<TargetGetTargetsHandler>();

		// Page
		//cdp.AddCommandHandler<PageGetFrameTreeHandler>();
		//cdp.AddCommandHandler<PageGetResourceTreeHandler>();
		//cdp.AddCommandHandler<PageGetNavigationHistoryHandler>();

		// DOM
		cdp.AddCommandHandler<DomEnableHandler>();
		cdp.AddCommandHandler<DomGetDocumentHandler>();
		cdp.AddCommandHandler<DomGetBoxModelHandler>();

		//cdp.AddCommandHandler<DomGetNodesForSubtreeByStyleHandler>();
		//cdp.AddCommandHandler<DomRequestChildNodesHandler>();
		//cdp.AddCommandHandler<DomSetInspectedNodeHandler>();
		//cdp.AddCommandHandler<DomPushNodesByBackendIdsToFrontendHandler>();
		//cdp.AddCommandHandler<DomResolveNodeHandler>();

		// CSS
		cdp.AddCommandHandler<CssEnableHandler>();
		cdp.AddCommandHandler<CssGetComputedStyleForNodeHandler>();
		cdp.AddCommandHandler<CssGetMatchedStylesForNodeHandler>();
		cdp.AddCommandHandler<CssGetInlineStylesForNodeHandler>();
		//cdp.AddCommandHandler<CssTrackComputedStyleUpdatesHandler>();
		//cdp.AddCommandHandler<CssTrackComputedStyleUpdatesForNodeHandler>();
		//cdp.AddCommandHandler<CssGetPlatformFontsForNodeHandler>();

		// Overlay
		//cdp.AddCommandHandler<OverlaySetShowGridOverlaysHandler>();
		//cdp.AddCommandHandler<OverlaySetShowFlexOverlaysHandler>();

		// Memory
		//cdp.AddCommandHandler<MemoryGetDomCountersHandler>();

		// whatever
		cdp.MapWhen(c => c.EndsWith(".enable"), ctx => ValueTask.FromResult(new CdpResult()));
		//cdp.AllowUnhandledCommands();

		return cdp;
	}
}
