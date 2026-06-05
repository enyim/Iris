using System.Text.Json;

using Enyim.Iris.Server.Protocol;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Enyim.Iris.Server.Dispatch;

/// <summary>Routes an incoming command to its registered handler and normalizes failures into CDP errors.</summary>
public interface ICdpDispatcher
{
	ValueTask<CdpResult> DispatchAsync(CdpCommandContext context);
}

/// <inheritdoc/>
public sealed class CdpDispatcher(ICdpCommandRegistry registry, ILogger<CdpDispatcher>? logger = null)
	: ICdpDispatcher
{
	private readonly ILogger _logger = logger ?? NullLogger<CdpDispatcher>.Instance;

	public async ValueTask<CdpResult> DispatchAsync(CdpCommandContext context)
	{
		if (!registry.TryGet(context.Method, out var handler))
		{
			if (registry.Fallback is not { } fallback)
				return CdpResult.Fail(CdpError.MethodNotFound(context.Method));
			handler = fallback;
		}

		try
		{
			return await handler(context).ConfigureAwait(false);
		}
		catch (CdpProtocolException ex)
		{
			return CdpResult.Fail(ex.Error);
		}
		catch (JsonException ex)
		{
			return CdpResult.Fail(CdpError.InvalidParams(ex.Message));
		}
		catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unhandled exception dispatching CDP method {Method}", context.Method);
			return CdpResult.Fail(CdpError.ServerError(ex.Message));
		}
	}
}
