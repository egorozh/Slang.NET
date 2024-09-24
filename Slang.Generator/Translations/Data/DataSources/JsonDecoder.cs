using System.Text.Json;

namespace Slang.Generator.Translations.Data.DataSources;

internal class JsonDecoder : BaseDecoder
{
    public override Dictionary<string, object?> Decode(string raw)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(raw)!;
    }
}