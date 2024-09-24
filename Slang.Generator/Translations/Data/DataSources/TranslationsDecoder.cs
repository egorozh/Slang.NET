namespace Slang.Generator.Translations.Data.DataSources;

public static class TranslationsDecoder
{
    /// Decodes with the specified file type
    public static Dictionary<string, object?> DecodeWithFileType(string json)
    {
        return new JsonDecoder().Decode(json);
    }
}