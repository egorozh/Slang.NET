using System.Globalization;
using Slang.Generator.Core.Entities;

namespace Slang.Generator.Core.Data;

/// Contains ALL translations of ALL locales
/// Represented as pure maps without modifications
///
/// locale -> translation map
public class TranslationComposition : Dictionary<CultureInfo, Dictionary<string, object?>>;

public abstract class TranslationsRepository
{
    /// This method transforms files to an intermediate model [TranslationComposition].
    public static async Task<TranslationComposition> Build(RawConfig rawConfig, SlangFileCollection fileCollection)
    {
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
                throw new Exception($"File: {file}\n{e}");
            }

            if (!translationComposition.ContainsKey(file.Locale))
            {
                translationComposition[file.Locale] = translations;
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