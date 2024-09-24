using System.Globalization;
using Slang.Generator.Files;

namespace Slang.Generator.Translations;

public class TranslationsMap(Dictionary<string, object?> translations)
    : Dictionary<string, object?>(translations);

/// Contains ALL translations of ALL locales
/// Represented as pure maps without modifications
///
/// locale -> translation map
public class TranslationComposition : Dictionary<CultureInfo, TranslationsMap>;

public abstract class TranslationsRepository
{
    /// This method transforms files to an intermediate model [TranslationComposition].
    public static async Task<TranslationComposition> Build(SlangFileCollection fileCollection)
    {
        var rawConfig = fileCollection.Config;

        TranslationComposition translationComposition = new();

        foreach (var file in fileCollection.Files)
        {
            string content = await file.Read();
            Dictionary<string, object?> translations;

            try
            {
                translations = TranslationsDecoder.DecodeWithFileType(content);
            }
            catch (Exception e)
            {
                throw new Exception($"File: {file.Path}\n{e}");
            }
            
            if (!translationComposition.ContainsKey(file.Locale))
            {
                translationComposition[file.Locale] = new TranslationsMap(translations);
            }
        }

        if (translationComposition.Keys.All(locale => !Equals(locale, rawConfig.BaseLocale)))
        {
            throw new Exception(
                $"Translation file for base locale \"{rawConfig.BaseLocale.TwoLetterISOLanguageName}\" not found.");
        }

        return translationComposition;
    }
}