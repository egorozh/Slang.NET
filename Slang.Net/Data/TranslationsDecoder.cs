using System.Text.Json;
using YamlDotNet.Serialization;

namespace Slang.Generator.Core.Data;

public static class TranslationsDecoder
{
    /// Decodes with the specified file type
    public static Dictionary<string, object?> DecodeWithFileType(string content, string type)
    {
        if (type == "yaml")
            return new Deserializer().Deserialize<Dictionary<string, object>>(content)!;

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(content)!;
    }
}