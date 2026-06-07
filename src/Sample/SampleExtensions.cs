using Enyim.Iris.Protocol;
using Enyim.Iris.Server;
using Enyim.Iris.Server.Hosting;
using Enyim.Iris.Server.Sessions;

using Sample.Inspection;

namespace Sample;

public static class SampleExtensions
{
	/// <summary>
	/// Registers the DebugNode inspection model (store, mapper) and all node-level CDP handlers.
	/// Call after <see cref="InspectionDomains.AddInspectionDomains"/> on the same builder.
	/// </summary>
	public static ICdpServerBuilder AddDebugNodeHandlers(this ICdpServerBuilder cdp)
	{
		cdp.Services.AddSingleton<IInspectionSnapshotStore, InspectionSnapshotStore>();
		cdp.Services.AddSingleton<DebugNodeMapper>();

		cdp.AddCommandHandler<DomGetDocumentHandler>();
		cdp.AddCommandHandler<DomGetBoxModelHandler>();
		cdp.AddCommandHandler<CssGetComputedStyleForNodeHandler>();
		cdp.AddCommandHandler<CssGetMatchedStylesForNodeHandler>();
		cdp.AddCommandHandler<CssGetInlineStylesForNodeHandler>();

		//cdp.AddCommandHandler<DomGetNodesForSubtreeByStyleHandler>();
		//cdp.AddCommandHandler<DomRequestChildNodesHandler>();
		//cdp.AddCommandHandler<DomSetInspectedNodeHandler>();
		//cdp.AddCommandHandler<DomPushNodesByBackendIdsToFrontendHandler>();
		//cdp.AddCommandHandler<DomResolveNodeHandler>();
		//cdp.AddCommandHandler<CssTrackComputedStyleUpdatesForNodeHandler>();
		//cdp.AddCommandHandler<CssGetPlatformFontsForNodeHandler>();
		//cdp.AddCommandHandler<MemoryGetDomCountersHandler>();

		return cdp;
	}

	/// <summary>
	/// Replaces the cached control-tree snapshot and nudges connected inspectors with
	/// <c>DOM.documentUpdated</c>. Non-blocking: enqueues via <c>TryWrite</c>.
	/// </summary>
	public static void PublishTree(this IDebugServer server, DebugNode root)
	{
		server.Services.GetRequiredService<IInspectionSnapshotStore>().SetTree(root);
		server.Services.GetRequiredService<ICdpSessionHub>().BroadcastAsync(new DOM.DocumentUpdated());
	}

	/// <summary>
	/// Broadcasts a log entry as <c>Log.entryAdded</c> to every session that has enabled the
	/// Log domain. Non-blocking: enqueues via <c>TryWrite</c>.
	/// </summary>
	public static void Log(this IDebugServer server, DebugLogEntry entry)
	{
		var hub = server.Services.GetRequiredService<ICdpSessionHub>();
		var mapper = server.Services.GetRequiredService<DebugNodeMapper>();
		hub.BroadcastAsync(mapper.MapLogEntry(entry));
	}
}
