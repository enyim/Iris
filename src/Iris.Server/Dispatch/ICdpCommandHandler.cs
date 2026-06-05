using ChromeProtocol.Core;

using Enyim.Iris.Server.Protocol;

namespace Enyim.Iris.Server.Dispatch;

/// <summary>A handler invoked for an incoming command. Implementations are resolved from DI.</summary>
public delegate ValueTask<CdpResult> CdpCommandDelegate(CdpCommandContext context);

/// <summary>
/// A raw command handler bound to a specific method. Use this for catch-all/passthrough handlers
/// or when you want full control over (de)serialization. For most cases prefer the strongly-typed
/// <see cref="ICdpCommandHandler{TParams, TResult}"/>.
/// </summary>
public interface ICdpCommandHandler
{
	/// <summary>The wire method this handler serves, e.g. <c>"Target.setDiscoverTargets"</c>.</summary>
	string Method { get; }

	ValueTask<CdpResult> HandleAsync(CdpCommandContext context);
}

/// <summary>
/// A strongly-typed command handler. <typeparamref name="TParams"/> is a generated command record
/// (which implements <see cref="ICommand{TResponse}"/>), so the method name and result type are
/// inferred from the contract. The framework deserializes params and serializes the result.
/// </summary>
public interface ICdpCommandHandler<in TParams, TResult>
	where TParams : ICommand<TResult>
	where TResult : IType
{
	ValueTask<TResult> HandleAsync(TParams parameters, CdpCommandContext context);
}
