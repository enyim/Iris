using Enyim.Iris.Server.Hosting;
using Enyim.Iris.Server.Inspection;

var debug = DebugServer.Create(o =>
{
	o.Port = 9333;
	o.TargetTitle = "Browser Emulator Sample";
	o.TargetUrl = "https://example.com/";
	o.BrowserName = "Chrome/124.0.6367.207";
	o.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 "
				   + "(KHTML, like Gecko) Chrome/124.0.6367.207 Safari/537.36";
	o.MemoryProvider = static () => new MemoryStats(
		GC.GetTotalMemory(false),
		GC.CollectionCount(0),
		GC.CollectionCount(1),
		GC.CollectionCount(2));
});

await debug.StartAsync();

// Push a static control tree so the Elements panel is populated immediately.
debug.PublishTree(new DebugNode(
	Id: "doc",
	Name: "#document",
	Kind: DebugNodeKind.Document,
	Children: [
		new DebugNode("html", "HTML", Children: [
			new DebugNode("head", "HEAD"),
			new DebugNode("body", "BODY", Children: [
				new DebugNode("h1", "H1",
					Attributes: [new("id", "title"), new("class", "main-heading")]),
				new DebugNode("p", "P"),
			]),
		]),
	]));

debug.Log(new DebugLogEntry(DebugLogLevel.Info, "DebugServer started", "App"));
debug.Log(new DebugLogEntry(DebugLogLevel.Info, $"Inspect at {debug.InspectUrl}", "App"));

Console.WriteLine($"DebugServer running. Open edge://inspect or chrome://inspect.");
Console.WriteLine($"Or navigate to: {debug.InspectUrl}");
Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

await debug.StopAsync();
await debug.DisposeAsync();
