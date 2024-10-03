using System.Text.Json;
using System.Text.Json.Serialization;

namespace Slang.Gpt.Domain.Utils;

public static class JsonHelpers
{
    public static string JsonEncode(Dictionary<string, object> dictionary)
    {
        return JsonSerializer.Serialize(dictionary, DictionaryContext.Default.DictionaryStringObject);
    }

    public static Dictionary<string, object?> JsonDecode(string json)
    {
        return JsonSerializer.Deserialize(json, DictionaryContext.Default.DictionaryStringObject)!;
    }
}

[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    WriteIndented = true
)]
internal partial class DictionaryContext : JsonSerializerContext;