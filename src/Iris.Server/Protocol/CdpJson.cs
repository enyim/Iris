using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ChromeProtocol.Core;

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
		Converters = { new FixedArrayTypeConverter() },
	};
}

// ChromeProtocol.Core.ArrayTypeConverter creates a new JsonArray from the existing Items nodes,
// which throws "The node already has a parent" because those JsonNode instances are still attached
// to the parsed response tree. Writing each node directly avoids the re-parenting.
file sealed class FixedArrayTypeConverter : JsonConverter<IArrayType?>
{
	public override bool CanConvert(Type objectType) => typeof(IArrayType).IsAssignableFrom(objectType);

	public override void Write(Utf8JsonWriter writer, IArrayType? value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		if (value is not null)
			foreach (var item in value.Items)
				if (item is null) writer.WriteNullValue();
				else item.WriteTo(writer);
		writer.WriteEndArray();
	}

	public override IArrayType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = JsonNode.Parse(ref reader);
		var items = node?.Deserialize<IReadOnlyCollection<JsonNode>>() ?? [];
		return Activator.CreateInstance(typeToConvert, items) as IArrayType;
	}
}
