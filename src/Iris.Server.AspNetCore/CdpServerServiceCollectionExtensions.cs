using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;
using Enyim.Iris.Server.Sessions;
using Enyim.Iris.Server.Targets;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>DI registration for the CDP server.</summary>
public static class CdpServerServiceCollectionExtensions
{
	/// <summary>
	/// Registers the CDP server services and returns a builder for mapping command handlers.
	/// The host must also call <c>app.UseWebSockets()</c> and <c>app.MapCdpServer()</c>.
	/// </summary>
	public static ICdpServerBuilder AddCdpServer(
		this IServiceCollection services,
		Action<CdpServerOptions>? configureOptions = null)
	{
		services.AddOptions<CdpServerOptions>();
		if (configureOptions is not null)
			services.Configure(configureOptions);

		var index = CdpContractIndex.Default;
		var registry = new CdpCommandRegistry();

		services.TryAddSingleton(index);
		services.TryAddSingleton(registry);
		services.TryAddSingleton<ICdpCommandRegistry>(registry);
		services.TryAddSingleton<ICdpDispatcher, CdpDispatcher>();
		services.TryAddSingleton<ICdpTargetRegistry, CdpTargetRegistry>();
		services.TryAddSingleton<ICdpSessionHub, CdpSessionHub>();
		services.TryAddSingleton<IInspectionSnapshotStore, InspectionSnapshotStore>();
		services.TryAddSingleton<DebugNodeMapper>();
		services.TryAddSingleton<CdpSessionFactory>();

		return new CdpServerBuilder(services, registry, index);
	}
}
