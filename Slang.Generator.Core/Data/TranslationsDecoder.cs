using System.Text.Json;

namespace Slang.Generator.Core.Data;

public static class TranslationsDecoder
{
    /// Decodes with the specified file type
    public static Dictionary<string, object?> DecodeWithFileType(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;
    }
}