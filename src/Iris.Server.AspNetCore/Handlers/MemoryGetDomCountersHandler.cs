using ChromeProtocol.Domains;

using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Inspection;

using Microsoft.Extensions.DependencyInjection;

namespace Enyim.Iris.Server.AspNetCore;

internal sealed class MemoryGetDomCountersHandler : ICdpCommandHandler<Memory.GetDOMCountersRequest, Memory.GetDOMCountersRequestResult>
{
	public ValueTask<Memory.GetDOMCountersRequestResult> HandleAsync(Memory.GetDOMCountersRequest parameters, CdpCommandContext context)
	{
		var provider = context.Services.GetService<Func<MemoryStats>>();
		if (provider is null)
			return new(new Memory.GetDOMCountersRequestResult(Documents: 0, Nodes: 0, JsEventListeners: 0));
		var stats = provider();
		return new(new Memory.GetDOMCountersRequestResult(
			Documents: (int)(stats.HeapBytes / (1024 * 1024)),
			Nodes: (int)stats.Gen0,
			JsEventListeners: (int)(stats.Gen1 + stats.Gen2)));
	}
}
