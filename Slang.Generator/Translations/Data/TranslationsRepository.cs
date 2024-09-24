using System.Globalization;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Files;
using Slang.Generator.Translations.Data.DataSources;
using Slang.Generator.Translations.Domain;

namespace Slang.Generator.Translations.Data;

public abstract class TranslationsRepository
{
    /// This method transforms files to an intermediate model [TranslationMap].
    /// After this step,
    /// - we removed the environment (i.e. dart:io, build_runner)
    /// - we removed the file type (JSON, YAML, CSV) because everything is a map now
    ///
    /// The resulting map is in a unmodified state, so no actual i18n handling (plural, rich text) has been applied.
    public static async Task<TranslationComposition> Build(SlangFileCollection fileCollection, bool verbose)
    {
        var rawConfig = fileCollection.Config;

        int padLeft = verbose
            ? _getPadLeft(
                files: fileCollection.Files,
                baseLocale: rawConfig.BaseLocale.TwoLetterISOLanguageName
            )
            : 0;

        TranslationComposition translationComposition = new();

        foreach (var file in fileCollection.Files)
        {
            string content = await file.Read();
            Dictionary<string, object> translations;

            try
            {
                translations = TranslationsDecoder.DecodeWithFileType(content);
            }
            catch (Exception e)
            {
                if (verbose)
                    Console.WriteLine("");

                throw new Exception($"File: {file.Path}\n{e}");
            }

            AddTranslations(translationComposition,
                locale: file.Locale,
                translations: translations);

            if (verbose)
                LogGettingTranslations(file.Locale, file, rawConfig, padLeft);
        }

        if (translationComposition.Keys.All(locale => !Equals(locale, rawConfig.BaseLocale)))
        {
            if (verbose)
                Console.WriteLine("");

            throw new Exception(
                $"Translation file for base locale \"{rawConfig.BaseLocale.TwoLetterISOLanguageName}\" not found.");
        }

        return translationComposition;
    }

    private static void LogGettingTranslations(CultureInfo locale,
        TranslationFile file,
        RawConfig rawConfig,
        int padLeft)
    {
        string baseLog = Equals(locale, rawConfig.BaseLocale) ? "(base) " : "";
        string namespaceLog = "";

        Console.WriteLine(
            $"\"{$"{baseLog}{namespaceLog}{file.Locale.TwoLetterISOLanguageName}".PadLeft(padLeft)}\" -> {file.Path}");
    }

    /// Add a namespace and its translations
    /// Namespace may be ignored if this feature is not used
    private static void AddTranslations(
        TranslationComposition composition,
        CultureInfo locale,
        Dictionary<string, object?> translations
    )
    {
        if (!composition.ContainsKey(locale))
        {
            // ensure that the locale exists
            composition[locale] = new TranslationsMap(translations);
        }
    }

    /// Determines the longest debug string used for PadLeft
    private static int _getPadLeft(
        List<TranslationFile> files,
        string baseLocale)
    {
        int longest = 0;

        foreach (var file in files)
        {
            int currLength = file.Locale.TwoLetterISOLanguageName.Length;

            if (file.Locale.TwoLetterISOLanguageName == baseLocale)
            {
                currLength += "(base) ".Length;
            }

            if (currLength > longest)
            {
                longest = currLength;
            }
        }

        // (base) locale
        return longest + 1; // only add first space
    }
}