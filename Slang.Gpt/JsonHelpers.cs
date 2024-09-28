using System.Text.Json;

namespace Slang.Gpt;

public static class JsonHelpers
{
    public static string JsonEncode(Dictionary<string, object> dictionary)
    {
        return JsonSerializer.Serialize(dictionary);
    }

    public static Dictionary<string, object> JsonDecode(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
    }
}