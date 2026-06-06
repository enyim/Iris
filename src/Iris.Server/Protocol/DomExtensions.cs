using Enyim.Iris.Protocol;

namespace Enyim.Iris.Server.Protocol;

public static class DomExtensions
{
	public static int RequiredValue(this DOM.NodeIdType? nodeId) =>
		nodeId is null
			? throw new CdpProtocolException(CdpError.NodeNotFound(0))
			: nodeId.Value;
}
