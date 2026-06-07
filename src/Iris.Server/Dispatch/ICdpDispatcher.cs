using Enyim.Iris.Server.Protocol;

namespace Enyim.Iris.Server.Dispatch;

/// <summary>Routes an incoming command to its registered handler and normalizes failures into CDP errors.</summary>
public interface ICdpDispatcher
{
	ValueTask<CdpResult> DispatchAsync(CdpCommandContext context);
}
