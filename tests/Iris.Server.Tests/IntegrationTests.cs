using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using Enyim.Iris.Server.AspNetCore;
using Enyim.Iris.Server.Hosting;

namespace Enyim.Iris.Server.Tests;

/// <summary>
/// End-to-end tests against an embedded <see cref="DebugServer"/> running on a loopback port.
/// Each fixture starts the server once and tears it down after all tests in the class run.
/// </summary>
public sealed class IntegrationTests : IAsyncLifetime
{
	private const int TestPort = 19333;
	private static readonly Uri BaseHttp = new($"http://127.0.0.1:{TestPort}");
	private static readonly Uri BaseWs = new($"ws://127.0.0.1:{TestPort}");

	private IDebugServer debug = null!;
	private HttpClient http = null!;

	private static CancellationToken Timeout => new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token;

	public async ValueTask InitializeAsync()
	{
		debug = DebugServer.Create(
			o =>
			{
				o.Port = TestPort;
				o.TargetTitle = "Test Target";
				o.TargetUrl = "https://example.com/";
				o.BrowserName = "Chrome/124.0.6367.207";
			},
			cdp => cdp
				.AddTarget(new Targets.CdpTarget
				{
					Id = "1",
					Url = "https://example.com/",
					Title = "Test Target",
					Type = "page"
				})
				.AddDefaultHandlers()
		);

		await debug.StartAsync();
		http = new HttpClient { BaseAddress = BaseHttp };
	}

	public async ValueTask DisposeAsync()
	{
		http.Dispose();
		await debug.StopAsync();
		await debug.DisposeAsync();
	}

	[Fact]
	public async Task Json_list_advertises_a_page_target()
	{
		using var doc = JsonDocument.Parse(await http.GetStringAsync("/json/list", Timeout));
		var target = Assert.Single(doc.RootElement.EnumerateArray().ToArray());
		Assert.Equal("page", target.GetProperty("type").GetString());
		Assert.StartsWith("ws", target.GetProperty("webSocketDebuggerUrl").GetString());
	}

	[Fact]
	public async Task Json_version_reports_browser_and_ws_url()
	{
		using var doc = JsonDocument.Parse(await http.GetStringAsync("/json/version", Timeout));
		Assert.Equal("Chrome/124.0.6367.207", doc.RootElement.GetProperty("Browser").GetString());
		Assert.Contains("/devtools/browser/", doc.RootElement.GetProperty("webSocketDebuggerUrl").GetString());
	}

	[Fact]
	public async Task Browser_getVersion_round_trips_over_websocket()
	{
		using var socket = await ConnectToPageAsync();
		await SendAsync(socket, """{"id":1,"method":"Browser.getVersion"}""");
		using var response = await ReceiveAsync(socket);
		Assert.Equal(1, response.RootElement.GetProperty("id").GetInt32());
		Assert.Equal("Chrome/124.0.6367.207", response.RootElement.GetProperty("result").GetProperty("product").GetString());
	}

	[Fact]
	public async Task Unhandled_command_returns_error_over_websocket()
	{
		using var socket = await ConnectToPageAsync();
		await SendAsync(socket, """{"id":2,"method":"Unknown.soVeryUnhandled"}""");
		using var response = await ReceiveAsync(socket);
		Assert.Equal(2, response.RootElement.GetProperty("id").GetInt32());
		Assert.True(response.RootElement.TryGetProperty("error", out _));
	}

	[Fact]
	public async Task Runtime_enable_emits_execution_context_and_responds()
	{
		using var socket = await ConnectToPageAsync();
		await SendAsync(socket, """{"id":3,"method":"Runtime.enable"}""");

		var sawEvent = false;
		var sawResponse = false;
		while (!sawResponse)
		{
			using var message = await ReceiveAsync(socket);
			var root = message.RootElement;
			if (root.TryGetProperty("method", out var method) &&
				method.GetString() == "Runtime.executionContextCreated")
			{
				sawEvent = true;
			}
			else if (root.TryGetProperty("id", out var id) && id.GetInt32() == 3)
			{
				sawResponse = true;
			}
		}

		Assert.True(sawEvent, "Expected Runtime.executionContextCreated after Runtime.enable");
	}

	private async Task<WebSocket> ConnectToPageAsync()
	{
		using var doc = JsonDocument.Parse(await http.GetStringAsync("/json/list", Timeout));
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
