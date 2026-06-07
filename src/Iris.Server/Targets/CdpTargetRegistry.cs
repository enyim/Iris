using System.Collections.Concurrent;

namespace Enyim.Iris.Server.Targets;

/// <summary>Thread-safe in-memory <see cref="ICdpTargetRegistry"/>.</summary>
public sealed class CdpTargetRegistry : ICdpTargetRegistry
{
	private readonly ConcurrentDictionary<string, CdpTarget> targets = new(StringComparer.Ordinal);

	// Dashed GUID to match the browser id format real Chrome/Edge report in /json/version.
	public string BrowserId { get; } = Guid.NewGuid().ToString();

	public IReadOnlyCollection<CdpTarget> GetTargets() => targets.Values.ToArray();

	public bool TryGet(string id, out CdpTarget target) => targets.TryGetValue(id, out target!);

	public CdpTarget Add(CdpTarget target)
	{
		targets[target.Id] = target;
		return target;
	}

	public bool Remove(string id) => targets.TryRemove(id, out _);

	public CdpTarget CreatePage(string url) =>
		Add(new CdpTarget
		{
			Id = Guid.NewGuid().ToString("N").ToUpperInvariant(),
			Type = "page",
			Title = String.IsNullOrEmpty(url) ? "New Tab" : url,
			Url = String.IsNullOrEmpty(url) ? "about:blank" : url,
		});
}
