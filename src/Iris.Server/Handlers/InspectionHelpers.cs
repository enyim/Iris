//using Enyim.Iris.Protocol;

//namespace Enyim.Iris.Server.Handlers;

//public static class InspectionHelpers
//{
//	public static Page.FrameType MakeFrame(string url) =>
//		new(
//			Id: new Page.FrameIdType("main-frame"),
//			LoaderId: new Network.LoaderIdType("loader-1"),
//			Url: url,
//			DomainAndRegistry: "",
//			SecurityOrigin: url,
//			MimeType: "text/html",
//			SecureContextType: new Page.SecureContextTypeType("InsecureScheme"),
//			CrossOriginIsolatedContextType: new Page.CrossOriginIsolatedContextTypeType("NotIsolated"),
//			GatedAPIFeatures: []);

//	public static DOM.NodeType EmptyDocument(string url) =>
//		new(
//			NodeId: new DOM.NodeIdType(1),
//			BackendNodeId: new DOM.BackendNodeIdType(1),
//			NodeTypeProperty: 9,
//			NodeName: "#document",
//			LocalName: "",
//			NodeValue: "",
//			DocumentURL: url,
//			BaseURL: url,
//			ChildNodeCount: 0);
//}
