using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using Enyim.Iris.Server;
using Enyim.Iris.Server.AspNetCore;
using Enyim.Iris.Server.Hosting;

using Sample.Inspection;

namespace Sample.Tests;

/// <summary>
/// End-to-end tests that exercise DebugNode-specific functionality (PublishTree, Log, GetDocument).
/// </summary>
public sealed class IntegrationTests : IAsyncLifetime
{
	private const int TestPort = 19334;
	private static readonly Uri BaseHttp = new($"http://127.0.0.1:{TestPort}");
	private static readonly Uri BaseWs = new($"ws://127.0.0.1:{TestPort}");

	private IDebugServer _debug = null!;
	private HttpClient _http = null!;

	private static CancellationToken Timeout => new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

	public async ValueTask InitializeAsync()
	{
		_debug = DebugServer.Create(
			o =>
			{
				o.Port = TestPort;
				o.TargetTitle = "Test Target";
				o.TargetUrl = "https://example.com/";
				o.BrowserName = "Chrome/124.0.6367.207";
			},
			cdp => cdp
				.AddInspectionTarget("https://example.com/", "Test Target")
				.AddDefaultHandlers()
				.AddDebugNodeHandlers());
		await _debug.StartAsync();
		_http = new HttpClient { BaseAddress = BaseHttp };
	}

	public async ValueTask DisposeAsync()
	{
		_http.Dispose();
		await _debug.StopAsync();
		await _debug.DisposeAsync();
	}

	[Fact]
	public async Task PublishTree_broadcasts_documentUpdated_to_connected_session()
	{
		using var socket = await ConnectToPageAsync();

		// Enable DOM so documentUpdated is not gated.
		await SendAsync(socket, """{"id":10,"method":"DOM.enable"}""");
		// Absorb the response.
		await ReceiveAsync(socket);

		_debug.PublishTree(new DebugNode(1, "#document", DebugNodeKind.Document));

		// Drain messages until we see documentUpdated or timeout.
		var sawEvent = false;
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
		while (!sawEvent && !cts.IsCancellationRequested)
		{
			using var msg = await ReceiveAsync(socket);
			if (msg.RootElement.TryGetProperty("method", out var m) &&
				m.GetString() == "DOM.documentUpdated")
				sawEvent = true;
		}
		Assert.True(sawEvent, "Expected DOM.documentUpdated after PublishTree");
	}

	[Fact]
	public async Task Log_broadcasts_entryAdded_to_sessions_with_log_enabled()
	{
		using var socket = await ConnectToPageAsync();

		await SendAsync(socket, """{"id":20,"method":"Log.enable"}""");
		await ReceiveAsync(socket); // absorb response

		_debug.Log(new DebugLogEntry(DebugLogLevel.Info, "hello from test", "Test"));

		var sawEvent = false;
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
		while (!sawEvent && !cts.IsCancellationRequested)
		{
			using var msg = await ReceiveAsync(socket);
			if (msg.RootElement.TryGetProperty("method", out var m) &&
				m.GetString() == "Log.entryAdded")
				sawEvent = true;
		}
		Assert.True(sawEvent, "Expected Log.entryAdded after Log(entry)");
	}

	[Fact]
	public async Task GetDocument_returns_mapped_tree_after_PublishTree()
	{
		_debug.PublishTree(new DebugNode(1, "#document", DebugNodeKind.Document,
			Children: [new DebugNode(2, "BODY")]));

		using var socket = await ConnectToPageAsync();
		await SendAsync(socket, """{"id":30,"method":"DOM.getDocument"}""");
		using var response = await ReceiveAsync(socket);

		var root = response.RootElement.GetProperty("result").GetProperty("root");
		Assert.Equal("#document", root.GetProperty("nodeName").GetString());
		Assert.Equal(1, root.GetProperty("childNodeCount").GetInt32());
	}

	[Fact]
	public async Task Log_is_not_delivered_to_session_that_never_enabled_Log()
	{
		using var socket = await ConnectToPageAsync();
		// Do NOT call Log.enable — log events should be gated.

		_debug.Log(new DebugLogEntry(DebugLogLevel.Info, "should not arrive", "Test"));

		// Send a known command and wait for its response; any event before that is unexpected.
		await SendAsync(socket, """{"id":99,"method":"Browser.getVersion"}""");
		using var response = await ReceiveAsync(socket);

		// The first message back must be the getVersion response, not a log event.
		Assert.True(response.RootElement.TryGetProperty("id", out var id));
		Assert.Equal(99, id.GetInt32());
	}

	private async Task<WebSocket> ConnectToPageAsync()
	{
		using var doc = JsonDocument.Parse(await _http.GetStringAsync("/json/list", Timeout));
		var id = doc.RootElement.EnumerateArray().First().GetProperty("id").GetString()!;
		var uri = new Uri(BaseWs, $"/devtools/page/{id}");
		var ws = new ClientWebSocket();
		await ws.ConnectAsync(uri, Timeout);
		return ws;
	}

	private static ValueTask SendAsync(WebSocket socket, string json) =>
		socket.SendAsync(Encoding.UTF8.GetBytes(json).AsMemory(),
			WebSocketMessageType.Text, endOfMessage: true, Timeout);

	private static async Task<JsonDocument> ReceiveAsync(WebSocket socket)
	{
		var buffer = new ArrayBufferWriter<byte>();
		while (true)
		{
			var result = await socket.ReceiveAsync(buffer.GetMemory(4096), Timeout);
			buffer.Advance(result.Count);
			if (result.EndOfMessage) break;
		}
		return JsonDocument.Parse(buffer.WrittenMemory);
	}
}
