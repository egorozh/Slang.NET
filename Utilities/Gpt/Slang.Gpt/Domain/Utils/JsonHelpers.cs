using System.Text.Json;

#if(NET7_0_OR_GREATER)
using System.Text.Json.Serialization;
#endif

namespace Slang.Gpt.Domain.Utils;

public static class JsonHelpers
{
#if(NET6_0)
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
#endif

    public static string JsonEncode(Dictionary<string, object?> dictionary)
    {
#if(NET7_0_OR_GREATER)
        return JsonSerializer.Serialize(dictionary!, DictionaryContext.Default.DictionaryStringObject);
#else
        return JsonSerializer.Serialize(dictionary, options: Options);
#endif
    }

    public static Dictionary<string, object?> JsonDecode(string json)
    {
#if(NET7_0_OR_GREATER)
        return JsonSerializer.Deserialize(json, DictionaryContext.Default.DictionaryStringObject)!;
#else
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;
#endif
    }
}

#if(NET7_0_OR_GREATER)
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    WriteIndented = true
)]
internal partial class DictionaryContext : JsonSerializerContext;
#endif