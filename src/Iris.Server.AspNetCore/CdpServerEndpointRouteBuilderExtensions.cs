using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>Maps the CDP HTTP discovery and WebSocket endpoints via <see cref="CdpDiscoveryController"/>.</summary>
public static class CdpServerEndpointRouteBuilderExtensions
{
	public static IEndpointRouteBuilder MapCdpServer(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapControllers();
		return endpoints;
	}
}
