using ChromeProtocol.Core;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Protocol;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Enyim.Iris.Server.AspNetCore;

/// <summary>Fluent surface for registering CDP command handlers during startup.</summary>
public interface ICdpServerBuilder
{
	IServiceCollection Services { get; }

	/// <summary>The method→type contract index used to resolve handler method names.</summary>
	CdpContractIndex Index { get; }

	/// <summary>Maps a method to an inline async handler typed against a generated command record.</summary>
	ICdpServerBuilder MapCommand<TParams, TResult>(Func<TParams, CdpCommandContext, ValueTask<TResult>> handler)
		where TParams : ICommand<TResult>
		where TResult : IType;

	/// <summary>Maps a method to an inline synchronous handler typed against a generated command record.</summary>
	ICdpServerBuilder MapCommand<TParams, TResult>(Func<TParams, CdpCommandContext, TResult> handler)
		where TParams : ICommand<TResult>
		where TResult : IType;

	/// <summary>Registers a raw handler delegate for an explicit method.</summary>
	ICdpServerBuilder MapRaw(string method, CdpCommandDelegate handler);

	/// <summary>
	/// Registers a predicate-based handler invoked when <paramref name="predicate"/> returns
	/// <see langword="true"/> for the method name. Checked in registration order after all
	/// typed handlers but before the fallback.
	/// </summary>
	ICdpServerBuilder MapWhen(Func<string, bool> predicate, CdpCommandDelegate handler);

	/// <summary>Sets the handler used for any command without a specific handler.</summary>
	ICdpServerBuilder MapFallback(CdpCommandDelegate handler);

	/// <summary>
	/// Makes unhandled commands return an empty success (<c>{}</c>) instead of "method not found".
	/// Useful when a real DevTools front-end issues many optional commands a stub server ignores.
	/// </summary>
	ICdpServerBuilder AllowUnhandledCommands();

	/// <summary>Registers a raw handler instance under its <see cref="ICdpCommandHandler.Method"/>.</summary>
	ICdpServerBuilder Map(ICdpCommandHandler handler);

	/// <summary>
	/// Registers a strongly-typed handler class (resolved per command from DI as scoped). The class
	/// must implement exactly one <see cref="ICdpCommandHandler{TParams, TResult}"/>.
	/// </summary>
	ICdpServerBuilder AddCommandHandler<THandler>() where THandler : class;
}

internal sealed class CdpServerBuilder(IServiceCollection services, CdpCommandRegistry registry, CdpContractIndex index)
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
