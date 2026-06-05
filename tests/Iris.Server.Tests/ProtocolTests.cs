using System.Buffers;
using System.Text;
using System.Text.Json;

using Enyim.Iris.Server.Protocol;

namespace Enyim.Iris.Server.Tests;

public class ProtocolTests
{
	private static ReadOnlyMemory<byte> Utf8(string s) => Encoding.UTF8.GetBytes(s);

	private static string Serialize(CdpOutgoingMessage message)
	{
		var buffer = new ArrayBufferWriter<byte>();
		message.WriteTo(buffer, CdpJson.Payload);
		return Encoding.UTF8.GetString(buffer.WrittenSpan);
	}

	[Fact]
	public void Parse_valid_command_extracts_fields()
	{
		var result = CdpWireParser.Parse(Utf8("""{"id":7,"method":"Page.navigate","params":{"url":"x"},"sessionId":"S1"}"""));

		Assert.False(result.IsError);
		Assert.Equal(7, result.Message.Id);
		Assert.Equal("Page.navigate", result.Message.Method);
		Assert.Equal("S1", result.Message.SessionId);
		Assert.Equal("x", result.Message.Params.GetProperty("url").GetString());
	}

	[Fact]
	public void Parse_keeps_params_valid_after_parse_returns()
	{
		// Params must be cloned/detached so it survives the source document being disposed.
		var result = CdpWireParser.Parse(Utf8("""{"id":1,"method":"X.y","params":{"a":1}}"""));
		GC.Collect();
		Assert.Equal(1, result.Message.Params.GetProperty("a").GetInt32());
	}

	[Fact]
	public void Parse_missing_method_is_invalid_request_but_keeps_id()
	{
		var result = CdpWireParser.Parse(Utf8("""{"id":42}"""));

		Assert.True(result.IsError);
		Assert.Equal((int)CdpErrorCode.InvalidRequest, result.Error!.Code);
		Assert.Equal(42, result.Id);
	}

	[Fact]
	public void Parse_invalid_json_is_parse_error()
	{
		var result = CdpWireParser.Parse(Utf8("not json"));

		Assert.True(result.IsError);
		Assert.Equal((int)CdpErrorCode.ParseError, result.Error!.Code);
	}

	[Fact]
	public void Result_message_serializes_with_id_and_result()
	{
		var json = Serialize(new CdpResultMessage(5, new { product = "DebugServer" }, "S2"));
		Assert.Equal("""{"id":5,"result":{"product":"DebugServer"},"sessionId":"S2"}""", json);
	}

	[Fact]
	public void Void_result_serializes_as_empty_object()
	{
		var json = Serialize(new CdpResultMessage(5, Result: null));
		Assert.Equal("""{"id":5,"result":{}}""", json);
	}

	[Fact]
	public void Error_message_serializes_error_object()
	{
		var json = Serialize(new CdpErrorMessage(9, CdpError.MethodNotFound("Foo.bar")));
		using var doc = JsonDocument.Parse(json);
		Assert.Equal(9, doc.RootElement.GetProperty("id").GetInt32());
		Assert.Equal(-32601, doc.RootElement.GetProperty("error").GetProperty("code").GetInt32());
	}

	[Fact]
	public void Event_message_serializes_method_and_params()
	{
		var json = Serialize(new CdpEventMessage("Runtime.executionContextCreated", new { context = 1 }));
		Assert.Equal("""{"method":"Runtime.executionContextCreated","params":{"context":1}}""", json);
	}
}
