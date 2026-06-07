using ChromeProtocol.Core;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Dispatch;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server;

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
