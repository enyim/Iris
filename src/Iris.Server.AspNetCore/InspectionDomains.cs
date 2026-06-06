using Enyim.Iris.Server.Inspection;
using Enyim.Iris.Server.Targets;

using ChromeProtocol.Domains;

using Microsoft.Extensions.DependencyInjection;

using System.Text.Json.Nodes;
using ChromeProtocol.Core;
using System.Text.Json.Serialization;

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
		// Browser
		cdp.MapCommand<Browser.GetVersionRequest, Browser.GetVersionRequestResult>((_, _) =>
			new Browser.GetVersionRequestResult(
				ProtocolVersion: "1.3",
				Product: "DebugServer/1.0",
				Revision: "0",
				UserAgent: "DebugServer/1.0 (Chrome DevTools Protocol)",
				JsVersion: "0.0.0"));

		// Target
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

		// Runtime
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

		// Debugger / Log / Network
		cdp.MapCommand<Debugger.EnableRequest, Debugger.EnableRequestResult>((_, _) =>
			new Debugger.EnableRequestResult(new Runtime.UniqueDebuggerIdType("debugger-1")));
		cdp.MapCommand<Log.EnableRequest, Log.EnableRequestResult>((_, _) => new Log.EnableRequestResult());
		cdp.MapCommand<Network.EnableRequest, Network.EnableRequestResult>((_, _) => new Network.EnableRequestResult());

		// DOM
		cdp.MapCommand<DOM.GetNodesForSubtreeByStyleRequest, DOM.GetNodesForSubtreeByStyleRequestResult>((_, ctx) =>
		{
			var store = ctx.Services.GetRequiredService<IInspectionSnapshotStore>();
			var mapper = ctx.Services.GetRequiredService<DebugNodeMapper>();
			var tree = store.CurrentTree;

			if (tree is null)
				return new DOM.GetNodesForSubtreeByStyleRequestResult([]);

			return new DOM.GetNodesForSubtreeByStyleRequestResult([]);
		});

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

		// Memory / HeapProfiler (optional provider)
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

		cdp.MapCommand<Overlay.SetShowGridOverlaysRequest, Overlay.SetShowGridOverlaysRequestResult>((r, ctx) => new Overlay.SetShowGridOverlaysRequestResult());
		cdp.MapCommand<Overlay.SetShowFlexOverlaysRequest, Overlay.SetShowFlexOverlaysRequestResult>((r, ctx) => new Overlay.SetShowFlexOverlaysRequestResult());

		cdp.MapCommand<DOM.EnableRequest, DOM.EnableRequestResult>((r, ctx) => new DOM.EnableRequestResult());
		cdp.MapCommand<CSS.EnableRequest, CSS.EnableRequestResult>((r, ctx) => new CSS.EnableRequestResult());
		cdp.MapCommand<Inspector.EnableRequest, Inspector.EnableRequestResult>((r, ctx) => new Inspector.EnableRequestResult());

		cdp.MapCommand<DOM.GetBoxModelRequest, DOM.GetBoxModelRequestResult>((req, ctx) =>
		{
			// look up your node by req.NodeId, get its bounds
			var (x, y, w, h) = (100, 100, 400, 200);

			var content = Quad(x, y, w, h);
			var padding = Quad(x, y, w, h);
			var border = Quad(x, y, w, h);
			var margin = Quad(x - 10, y - 10, w + 20, h + 20);

			return new DOM.GetBoxModelRequestResult(new DOM.BoxModelType(
				Content: content,
				Padding: padding,
				Border: border,
				Margin: margin,
				Width: (int)w,
				Height: (int)h));

			static DOM.QuadType Quad(double x, double y, double w, double h) => new([x, y, x + w, y, x + w, y + h, x, y + h]);
		});


		cdp.MapCommand<CSS.GetComputedStyleForNodeRequest, CSS.GetComputedStyleForNodeRequestResult>((req, ctx) =>
		{
			var attrs = new[]
			{
				("display", "block"),
				("width", "400px"),
				("height", "200px"),

				("border-left-width", "0px"),
				("border-top-width", "0px"),
				("border-bottom-width", "0px"),
				("border-right-width", "0px"),

				("padding-left", "0px"),
				("padding-top", "0px"),
				("padding-bottom", "0px"),
				("padding-right", "0px"),

				("margin-left", "10px"),
				("margin-top", "10px"),
				("margin-bottom", "10px"),
				("margin-right", "10px"),

				("box-sizing", "content-box"),
				("position", "static"),
			};

			return new CSS.GetComputedStyleForNodeRequestResult(attrs.Select(a => new CSS.CSSComputedStylePropertyType(a.Item1, a.Item2)).ToArray());
		});

		cdp.MapCommand<CSS.GetMatchedStylesForNodeRequest, CSS.GetMatchedStylesForNodeRequestResult>((r, ctx) => new CSS.GetMatchedStylesForNodeRequestResult());
		cdp.MapCommand<CSS.TrackComputedStyleUpdatesRequest, CSS.TrackComputedStyleUpdatesRequestResult>((r, ctx) => new CSS.TrackComputedStyleUpdatesRequestResult());
		cdp.MapCommand<CSS.TrackComputedStyleUpdatesRequest, CSS.TrackComputedStyleUpdatesRequestResult>((r, ctx) => new CSS.TrackComputedStyleUpdatesRequestResult());
		cdp.MapCommand<DOM.SetInspectedNodeRequest, DOM.SetInspectedNodeRequestResult>((r, ctx) => new DOM.SetInspectedNodeRequestResult());
		cdp.MapCommand<CSS.GetPlatformFontsForNodeRequest, CSS.GetPlatformFontsForNodeRequestResult>((r, ctx) => new CSS.GetPlatformFontsForNodeRequestResult([]));
		cdp.MapCommand<CSS.TrackComputedStyleUpdatesRequest, CSS.TrackComputedStyleUpdatesRequestResult>((r, ctx) => new CSS.TrackComputedStyleUpdatesRequestResult());
		cdp.MapCommand<TrackComputedStyleUpdatesForNodeRequest, TrackComputedStyleUpdatesForNodeRequestResult>((r, ctx) => new TrackComputedStyleUpdatesForNodeRequestResult());
		cdp.MapCommand<DOM.PushNodesByBackendIdsToFrontendRequest, DOM.PushNodesByBackendIdsToFrontendRequestResult>((r, ctx) => new DOM.PushNodesByBackendIdsToFrontendRequestResult([]));

		cdp.MapCommand<CSS.GetInlineStylesForNodeRequest, CSS.GetInlineStylesForNodeRequestResult>((req, ctx) => new CSS.GetInlineStylesForNodeRequestResult());
		cdp.MapCommand<DOM.ResolveNodeRequest, DOM.ResolveNodeRequestResult>(async (r, ctx) => throw new NotImplementedException());


		//cdp.AllowUnhandledCommands();


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

[MethodName("CSS.trackComputedStyleUpdatesForNode")]
public record TrackComputedStyleUpdatesForNodeRequest([property: JsonPropertyName("nodeId")] DOM.NodeIdType NodeId) : ICommand<TrackComputedStyleUpdatesForNodeRequestResult>, ICommand;

public record TrackComputedStyleUpdatesForNodeRequestResult : IType;

