using System.Text.Json;
using System.Text.Json.Serialization;

namespace Enyim.Iris.Server.Protocol;

/// <summary>
/// Shared <see cref="JsonSerializerOptions"/> for CDP payloads. The generated
/// <c>ChromeProtocol.Domains</c> records already carry <see cref="JsonPropertyNameAttribute"/>
/// values matching the camelCase wire format, so no naming policy is required here.
/// </summary>
public static class CdpJson
{
	/// <summary>Options used to (de)serialize command params and command/event results.</summary>
	public static readonly JsonSerializerOptions Payload = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString,
	};
}
