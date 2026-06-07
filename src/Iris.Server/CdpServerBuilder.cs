using ChromeProtocol.Core;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Protocol;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Enyim.Iris.Server;

public sealed class CdpServerBuilder(IServiceCollection services, CdpCommandRegistry registry, CdpContractIndex index)
	: ICdpServerBuilder
{
	public IServiceCollection Services => services;

	public CdpContractIndex Index => index;

	public ICdpServerBuilder MapCommand<TParams, TResult>(Func<TParams, CdpCommandContext, ValueTask<TResult>> handler)
		where TParams : ICommand<TResult>
		where TResult : IType
	{
		registry.MapCommand(handler, index);
		return this;
	}

	public ICdpServerBuilder MapCommand<TParams, TResult>(Func<TParams, CdpCommandContext, TResult> handler)
		where TParams : ICommand<TResult>
		where TResult : IType
	{
		registry.MapCommand(handler, index);
		return this;
	}

	public ICdpServerBuilder MapRaw(string method, CdpCommandDelegate handler)
	{
		registry.Map(method, handler);
		return this;
	}

	public ICdpServerBuilder MapWhen(Func<string, bool> predicate, CdpCommandDelegate handler)
	{
		registry.MapWhen(predicate, handler);
		return this;
	}

	public ICdpServerBuilder MapFallback(CdpCommandDelegate handler)
	{
		registry.Fallback = handler;
		return this;
	}

	public ICdpServerBuilder AllowUnhandledCommands() =>
		MapFallback(static _ => new ValueTask<CdpResult>(CdpResult.Ok()));

	public ICdpServerBuilder Map(ICdpCommandHandler handler)
	{
		registry.Map(handler);
		return this;
	}

	public ICdpServerBuilder AddCommandHandler<THandler>() where THandler : class
	{
		var iface = Array.Find(
			typeof(THandler).GetInterfaces(),
			i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICdpCommandHandler<,>))
			?? throw new InvalidOperationException(
				$"{typeof(THandler)} must implement ICdpCommandHandler<TParams, TResult>.");

		services.TryAddScoped<THandler>();

		var args = iface.GetGenericArguments();
		// Reuse the strongly-typed generic registration so dispatch stays allocation-light.
		typeof(CdpCommandRegistryExtensions)
			.GetMethod(nameof(CdpCommandRegistryExtensions.MapHandler))!
			.MakeGenericMethod(typeof(THandler), args[0], args[1])
			.Invoke(null, [registry, index]);

		return this;
	}
}
