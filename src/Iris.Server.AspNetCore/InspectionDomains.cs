using Enyim.Iris.Server.Inspection;
using Enyim.Iris.Server.Targets;

using ChromeProtocol.Domains;

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
	public static ICdpServerBuilder AddInspectionDomains(
		this ICdpServerBuilder cdp,
		string targetUrl = "app://main-window",
		string targetTitle = "Debug Target")
	{
		// --- Browser ---
		cdp.MapCommand<Browser.GetVersionRequest, Browser.GetVersionRequestResult>((_, _) =>
			new Browser.GetVersionRequestResult(
				ProtocolVersion: "1.3",
				Product: "DebugServer/1.0",
				Revision: "0",
				UserAgent: "DebugServer/1.0 (Chrome DevTools Protocol)",
				JsVersion: "0.0.0"));

		// --- Target ---
		cdp.MapCommand<Target.SetDiscoverTargetsRequest, Target.SetDiscoverTargetsRequestResult>(
			(_, _) => new Target.SetDiscoverTargetsRequestResult());

		cdp.MapCommand<Target.GetTargetsRequest, Target.GetTargetsRequestResult>((_, ctx) =>
		{
			var registry = ctx.Services.GetRequiredService<ICdpTargetRegistry>();
			var infos = registry.GetTargets()
				.Select(t => new Target.TargetInfoType(
					TargetId: new Target.TargetIDType(t.Id),
					Type: t.Type,
					Title: t.Title,
					Url: t.Url,
					Attached: true,
					CanAccessOpener: false))
				.ToArray();
			return new Target.GetTargetsRequestResult(infos);
		});

		// --- Page ---
		cdp.MapCommand<Page.EnableRequest, Page.EnableRequestResult>((_, _) => new Page.EnableRequestResult());

		cdp.MapCommand<Page.GetFrameTreeRequest, Page.GetFrameTreeRequestResult>((_, _) =>
			new Page.GetFrameTreeRequestResult(new Page.FrameTreeType(MakeFrame(targetUrl))));

		cdp.MapCommand<Page.GetResourceTreeRequest, Page.GetResourceTreeRequestResult>((_, _) =>
			new Page.GetResourceTreeRequestResult(
				new Page.FrameResourceTreeType(MakeFrame(targetUrl), Resources: [])));

		cdp.MapCommand<Page.GetNavigationHistoryRequest, Page.GetNavigationHistoryRequestResult>((_, _) =>
			new Page.GetNavigationHistoryRequestResult(
				CurrentIndex: 0,
				Entries: [new Page.NavigationEntryType(
					1, targetUrl, targetUrl, targetTitle,
					new Page.TransitionTypeType("typed"))]));

		// --- Runtime ---
		cdp.MapCommand<Runtime.EnableRequest, Runtime.EnableRequestResult>(async (_, ctx) =>
		{
			await ctx.Events.EmitAsync(
				new Runtime.ExecutionContextCreated(
					new Runtime.ExecutionContextDescriptionType(
						Id: new Runtime.ExecutionContextIdType(1),
						Origin: "://",
						Name: "DebugServer",
						UniqueId: "context-1")),
				cancellationToken: ctx.CancellationToken);
			return new Runtime.EnableRequestResult();
		});

		// --- Debugger / Log / Network ---
		cdp.MapCommand<Debugger.EnableRequest, Debugger.EnableRequestResult>((_, _) =>
			new Debugger.EnableRequestResult(new Runtime.UniqueDebuggerIdType("debugger-1")));
		cdp.MapCommand<Log.EnableRequest, Log.EnableRequestResult>((_, _) => new Log.EnableRequestResult());
		cdp.MapCommand<Network.EnableRequest, Network.EnableRequestResult>((_, _) => new Network.EnableRequestResult());

		// --- DOM ---
		cdp.MapCommand<DOM.GetDocumentRequest, DOM.GetDocumentRequestResult>((_, ctx) =>
		{
			var store = ctx.Services.GetRequiredService<IInspectionSnapshotStore>();
			var mapper = ctx.Services.GetRequiredService<DebugNodeMapper>();
			var tree = store.CurrentTree;

			if (tree is null)
				return new DOM.GetDocumentRequestResult(EmptyDocument(targetUrl));

			return new DOM.GetDocumentRequestResult(mapper.MapTree(tree));
		});

		// DevTools may ask for child nodes of a specific node; full tree is already in getDocument.
		cdp.MapCommand<DOM.RequestChildNodesRequest, DOM.RequestChildNodesRequestResult>(
			(_, _) => new DOM.RequestChildNodesRequestResult());

		// --- Memory / HeapProfiler (optional provider) ---
		cdp.MapCommand<Memory.GetDOMCountersRequest, Memory.GetDOMCountersRequestResult>((_, ctx) =>
		{
			var provider = ctx.Services.GetService<Func<MemoryStats>>();
			if (provider is null)
				return new Memory.GetDOMCountersRequestResult(Documents: 0, Nodes: 0, JsEventListeners: 0);
			var stats = provider();
			return new Memory.GetDOMCountersRequestResult(
				Documents: (int)(stats.HeapBytes / (1024 * 1024)),
				Nodes: (int)stats.Gen0,
				JsEventListeners: (int)(stats.Gen1 + stats.Gen2));
		});

		cdp.AllowUnhandledCommands();

		return cdp;
	}

	private static Page.FrameType MakeFrame(string url) =>
		new(
			Id: new Page.FrameIdType("main-frame"),
			LoaderId: new Network.LoaderIdType("loader-1"),
			Url: url,
			DomainAndRegistry: "",
			SecurityOrigin: url,
			MimeType: "text/html",
			SecureContextType: new Page.SecureContextTypeType("InsecureScheme"),
			CrossOriginIsolatedContextType: new Page.CrossOriginIsolatedContextTypeType("NotIsolated"),
			GatedAPIFeatures: []);

	private static DOM.NodeType EmptyDocument(string url) =>
		new(
			NodeId: new DOM.NodeIdType(1),
			BackendNodeId: new DOM.BackendNodeIdType(1),
			NodeTypeProperty: 9,
			NodeName: "#document",
			LocalName: "",
			NodeValue: "",
			DocumentURL: url,
			BaseURL: url,
			ChildNodeCount: 0);
}
