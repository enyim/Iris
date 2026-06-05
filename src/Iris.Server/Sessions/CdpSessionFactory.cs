using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Transport;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Enyim.Iris.Server.Sessions;

/// <summary>Creates <see cref="CdpSession"/> instances wired to the shared server services.</summary>
public sealed class CdpSessionFactory(
	ICdpDispatcher dispatcher,
	CdpContractIndex index,
	IServiceScopeFactory scopeFactory,
	ILoggerFactory loggerFactory,
	ICdpSessionHub hub)
{
	private readonly ILogger<CdpSession> _logger = loggerFactory.CreateLogger<CdpSession>();

	public CdpSession Create(ICdpConnection connection, string connectionId, string? sessionId = null) =>
		new(connection, dispatcher, index, scopeFactory, _logger, connectionId, sessionId, hub: hub);
}
