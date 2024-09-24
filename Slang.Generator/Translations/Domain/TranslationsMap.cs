namespace Slang.Generator.Translations.Domain;

public class TranslationsMap(Dictionary<string, object?> translations) 
    : CustomDictionary<string, object>(translations)
{
}