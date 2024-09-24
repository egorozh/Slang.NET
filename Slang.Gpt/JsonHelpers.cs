using System.Text.Json;

namespace Slang.Gpt;

public static class JsonHelpers
{
    public static string jsonEncode(Dictionary<string, object> dictionary)
    {
        return JsonSerializer.Serialize(dictionary);
    }

    public static Dictionary<string, object> jsonDecode(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
    }
}