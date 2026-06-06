using System.Runtime.CompilerServices;

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
	Id: 1,
	Name: "#document",
	Kind: DebugNodeKind.Document,
	Children: [
		new DebugNode(2, "HTML", Children: [
			new DebugNode(3, "HEAD"),
			new DebugNode(4, "BODY", Children: [
				new DebugNode(5, "H1",
					ComputedStyle: GetStyles(),
					Attributes: new Dictionary<string, string> { ["id"] = "title", ["class"] = "main-heading" }),
				new DebugNode(6, "button", Children: [ new DebugNode(7, "helo", Kind: DebugNodeKind.Text) ]),
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

Dictionary<string, string> GetStyles()
{
	return new()
	{
			{"display", "block" },
			{"position", "static" },
			{"width", "400px"},
			{"height", "200px"},
			{"border-left-width", "0px"},
			{"border-top-width", "0px"},
			{"border-bottom-width", "0px"},
			{"border-right-width", "0px"},
			{"padding-left", "0px"},
			{"padding-top", "0px"},
			{"padding-bottom", "0px"},
			{"padding-right", "0px"},
			{"margin-left", "10px"},
			{"margin-top", "10px"},
			{"margin-bottom", "10px"},
			{"margin-right", "10px"},
	};
}
