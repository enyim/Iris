using Enyim.Iris.Server.Dispatch;
using Enyim.Iris.Server.Protocol;

using ChromeProtocol.Domains;

namespace Enyim.Iris.Server.Tests;

public class DispatcherTests
{
	private static (ICdpDispatcher Dispatcher, CdpCommandRegistry Registry) NewDispatcher()
	{
		var registry = new CdpCommandRegistry();
		return (new CdpDispatcher(registry), registry);
	}

	[Fact]
	public async Task Unknown_method_returns_method_not_found()
	{
		var (dispatcher, _) = NewDispatcher();

		var result = await dispatcher.DispatchAsync(TestContext.Create("Nope.nope"));

		Assert.True(result.IsError);
		Assert.Equal((int)CdpErrorCode.MethodNotFound, result.Error!.Code);
	}

	[Fact]
	public async Task Typed_handler_runs_and_returns_result()
	{
		var (dispatcher, registry) = NewDispatcher();
		registry.MapCommand<Browser.GetVersionRequest, Browser.GetVersionRequestResult>((_, _) =>
			new Browser.GetVersionRequestResult("1.3", "DebugServer", "0", "UA", "0"));

		var result = await dispatcher.DispatchAsync(TestContext.Create("Browser.getVersion"));

		Assert.False(result.IsError);
		var payload = Assert.IsType<Browser.GetVersionRequestResult>(result.Result);
		Assert.Equal("DebugServer", payload.Product);
	}

	[Fact]
	public async Task Malformed_params_returns_invalid_params()
	{
		var (dispatcher, registry) = NewDispatcher();
		registry.MapCommand<Browser.GetVersionRequest, Browser.GetVersionRequestResult>((_, _) =>
			new Browser.GetVersionRequestResult("1.3", "DebugServer", "0", "UA", "0"));

		// A JSON number cannot bind to the command record -> JsonException -> InvalidParams.
		var ctx = TestContext.Create("Browser.getVersion", TestContext.Json("123"));
		var result = await dispatcher.DispatchAsync(ctx);

		Assert.True(result.IsError);
		Assert.Equal((int)CdpErrorCode.InvalidParams, result.Error!.Code);
	}

	[Fact]
	public async Task Handler_throwing_protocol_exception_maps_to_its_error()
	{
		var (dispatcher, registry) = NewDispatcher();
		registry.Map("Custom.boom", _ =>
			throw new CdpProtocolException(CdpError.ServerError("boom")));

		var result = await dispatcher.DispatchAsync(TestContext.Create("Custom.boom"));

		Assert.True(result.IsError);
		Assert.Equal((int)CdpErrorCode.ServerError, result.Error!.Code);
		Assert.Equal("boom", result.Error.Message);
	}

	[Fact]
	public async Task Unexpected_exception_maps_to_server_error()
	{
		var (dispatcher, registry) = NewDispatcher();
		registry.Map("Custom.throw", _ => throw new InvalidOperationException("kaboom"));

		var result = await dispatcher.DispatchAsync(TestContext.Create("Custom.throw"));

		Assert.True(result.IsError);
		Assert.Equal((int)CdpErrorCode.ServerError, result.Error!.Code);
	}
}
