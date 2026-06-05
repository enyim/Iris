namespace Enyim.Iris.Server.Inspection;

public enum DebugNodeKind { Element, Document, Text }

/// <summary>
/// Neutral, CDP-free representation of a node in the host app's control tree. The host app
/// builds a <see cref="DebugNode"/> graph and pushes it via <c>IDebugServer.PublishTree</c>;
/// the server maps it to DOM types internally.
/// </summary>
public sealed record DebugNode(
	string Id,
	string Name,
	DebugNodeKind Kind = DebugNodeKind.Element,
	IReadOnlyList<KeyValuePair<string, string>>? Attributes = null,
	IReadOnlyList<DebugNode>? Children = null);

public enum DebugLogLevel { Verbose, Info, Warning, Error }

public readonly record struct DebugLogEntry(
	DebugLogLevel Level,
	string Text,
	string? Source = null,
	DateTimeOffset? Timestamp = null);

public readonly record struct MemoryStats(long HeapBytes, long Gen0, long Gen1, long Gen2);
