using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Slang.Gpt;

public static class FileUtils
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,

        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static void WriteFileOfType(string path, Dictionary<string, object?> content)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(content, Options));
    }
}