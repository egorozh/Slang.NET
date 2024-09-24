namespace Slang.Generator.Translations.Data.DataSources;

internal abstract class BaseDecoder
{
    public abstract Dictionary<string, object?> Decode(string raw);
}