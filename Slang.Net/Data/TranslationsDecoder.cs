using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

namespace Slang.Generator.Core.Data;

public static class TranslationsDecoder
{
    /// Decodes with the specified file type
    public static Dictionary<string, object?> DecodeWithFileType(string content, string type)
    {
        if (type == "yaml")
            return new DeserializerBuilder()
                .AddSystemTextJson()
                .Build()
                .Deserialize<Dictionary<string, object>>(content)!;

        return JsonSerializer.Deserialize<Dictionary<string, object?>>(content)!;
    }
}