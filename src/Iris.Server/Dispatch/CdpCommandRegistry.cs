using System.Text.Json;

using ChromeProtocol.Core;

using Enyim.Iris.Server.Contracts;
using Enyim.Iris.Server.Protocol;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.Dispatch;

/// <summary>Maps CDP method names to handler delegates. Populated at configuration time.</summary>
public interface ICdpCommandRegistry
{
	/// <summary>Registers (or replaces) the handler for a method.</summary>
	ICdpCommandRegistry Map(string method, CdpCommandDelegate handler);

	/// <summary>
	/// Registers a predicate-based handler invoked when <paramref name="predicate"/> returns
	/// <see langword="true"/> for the method name. Checked in registration order, after all
	/// exact-match handlers but before the <see cref="Fallback"/>.
	/// </summary>
	ICdpCommandRegistry MapWhen(Func<string, bool> predicate, CdpCommandDelegate handler);

	bool TryGet(string method, out CdpCommandDelegate handler);

	IReadOnlyCollection<string> Methods { get; }

	/// <summary>
	/// Optional handler invoked when no method-specific handler is registered. When null, unknown
	/// methods return <see cref="CdpErrorCode.MethodNotFound"/>.
	/// </summary>
	CdpCommandDelegate? Fallback { get; set; }
}

/// <inheritdoc/>
public sealed class CdpCommandRegistry : ICdpCommandRegistry
{
	private readonly Dictionary<string, CdpCommandDelegate> _handlers = new(StringComparer.Ordinal);
	private readonly List<(Func<string, bool> Predicate, CdpCommandDelegate Handler)> _predicateHandlers = [];

	public CdpCommandDelegate? Fallback { get; set; }

	public ICdpCommandRegistry Map(string method, CdpCommandDelegate handler)
	{
		ArgumentException.ThrowIfNullOrEmpty(method);
		_handlers[method] = handler;
		return this;
	}

	public ICdpCommandRegistry MapWhen(Func<string, bool> predicate, CdpCommandDelegate handler)
	{
		ArgumentNullException.ThrowIfNull(predicate);
		ArgumentNullException.ThrowIfNull(handler);
		_predicateHandlers.Add((predicate, handler));
		return this;
	}

	public bool TryGet(string method, out CdpCommandDelegate handler)
	{
		if (_handlers.TryGetValue(method, out handler!))
			return true;

		foreach (var (predicate, predicateHandler) in _predicateHandlers)
		{
			if (predicate(method))
			{
				handler = predicateHandler;
				return true;
			}
		}

		handler = null!;
		return false;
	}

	public IReadOnlyCollection<string> Methods => _handlers.Keys;
}

/// <summary>Strongly-typed registration helpers built on the generated contracts.</summary>
public static class CdpCommandRegistryExtensions
{
	/// <summary>Maps a method to an inline async handler typed against a generated command record.</summary>
	public static ICdpCommandRegistry MapCommand<TParams, TResult>(
		this ICdpCommandRegistry registry,
		Func<TParams, CdpCommandContext, ValueTask<TResult>> handler,
		CdpContractIndex? index = null)
		where TParams : ICommand<TResult>
		where TResult : IType
	{
		var method = (index ?? CdpContractIndex.Default).GetMethodName(typeof(TParams));
		return registry.Map(method, async ctx =>
		{
			if (!TryBind<TParams>(ctx, out var parameters, out var error))
				return CdpResult.Fail(error);
			var result = await handler(parameters, ctx).ConfigureAwait(false);
			return CdpResult.Ok(result);
		});
	}

	/// <summary>Maps a method to an inline synchronous handler typed against a generated command record.</summary>
	public static ICdpCommandRegistry MapCommand<TParams, TResult>(
		this ICdpCommandRegistry registry,
		Func<TParams, CdpCommandContext, TResult> handler,
		CdpContractIndex? index = null)
		where TParams : ICommand<TResult>
		where TResult : IType =>
		registry.MapCommand<TParams, TResult>((p, ctx) => new ValueTask<TResult>(handler(p, ctx)), index);

	/// <summary>Registers a raw handler instance under its declared <see cref="ICdpCommandHandler.Method"/>.</summary>
	public static ICdpCommandRegistry Map(this ICdpCommandRegistry registry, ICdpCommandHandler handler) =>
		registry.Map(handler.Method, handler.HandleAsync);

	/// <summary>Maps a method to a DI-resolved strongly-typed handler service.</summary>
	public static ICdpCommandRegistry MapHandler<THandler, TParams, TResult>(
		this ICdpCommandRegistry registry,
		CdpContractIndex? index = null)
		where THandler : ICdpCommandHandler<TParams, TResult>
		where TParams : ICommand<TResult>
		where TResult : IType
	{
		var method = (index ?? CdpContractIndex.Default).GetMethodName(typeof(TParams));
		return registry.Map(method, async ctx =>
		{
			if (!TryBind<TParams>(ctx, out var parameters, out var error))
				return CdpResult.Fail(error);
			var handler = ctx.Services.GetRequiredService<THandler>();
			var result = await handler.HandleAsync(parameters, ctx).ConfigureAwait(false);
			return CdpResult.Ok(result);
		});
	}

	private static bool TryBind<TParams>(CdpCommandContext ctx, out TParams parameters, out CdpError error)
	{
		try
		{
			parameters = ctx.DeserializeParams<TParams>();
			error = null!;
			return true;
		}
		catch (JsonException ex)
		{
			parameters = default!;
			error = CdpError.InvalidParams(ex.Message);
			return false;
		}
	}
}
