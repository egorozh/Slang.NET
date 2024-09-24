using System.Globalization;
using Slang.Generator.Files;
using Slang.Generator.Translations.Data.DataSources;
using Slang.Gpt.Data;
using Slang.Gpt.Models;
using Slang.Gpt.Utils;

namespace Slang.Gpt;

internal record TranslateMetrics(
    int EndPromptCount,
    int InputTokens,
    int OutputTokens);

internal static class SlangGptTranslator
{
    /// <summary>
    /// Translates a file to a target locale.
    /// </summary>
    public static async Task<TranslateMetrics> Translate(
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        CultureInfo targetLocale,
        string outDir,
        bool full,
        bool debug,
        TranslationFile file,
        Dictionary<string, object> originalTranslations,
        string apiKey,
        int promptCount
    )
    {
        Console.WriteLine("");
        Console.WriteLine(
            $"Translating <{file.Locale.TwoLetterISOLanguageName}> to <{targetLocale.TwoLetterISOLanguageName}> for {file.Path} ...");

        // existing translations of target locale
        Dictionary<string, object> existingTranslations = [];

        string targetPath = PathUtils.WithFileName(
            directoryPath: outDir,
            fileName:
            $"{targetLocale.TwoLetterISOLanguageName}{fileCollection.Config.InputFilePattern}",
            pathSeparator: Path.PathSeparator
        );

        if (!full)
        {
            foreach (var destFile in fileCollection.Files)
            {
                if ((destFile.NamespaceString == file.NamespaceString) &&
                    Equals(destFile.Locale, targetLocale))
                {
                    string raw = await destFile.Read();
                    try
                    {
                        existingTranslations = TranslationsDecoder.DecodeWithFileType(raw);
                        targetPath = destFile.Path;
                        Console.WriteLine($" -> With partial translations from {destFile.Path}");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"File: {destFile.Path}\n{e}");
                    }

                    break;
                }
            }
        }

        var inputTranslations = MapUtils.Subtract(
            target: originalTranslations,
            other: existingTranslations
        );

        // We assume that these translations already exists in the target locale.
        Maps.removeIgnoreGpt(map: inputTranslations);

        // extract original comments but keep them in the inputTranslations
        // we will add the original comments later again
        var comments = Maps.extractComments(
            map: inputTranslations,
            remove: false
        );

        if (inputTranslations.Count == 0)
        {
            Console.WriteLine(" -> No new translations");

            return new TranslateMetrics(
                EndPromptCount: promptCount,
                InputTokens: 0,
                OutputTokens: 0
            );
        }

        var prompts = Prompt.Prompt.GetPrompts(
            rawConfig: fileCollection.Config,
            targetLocale: targetLocale,
            config: gptConfig,
            namespaceStroke: null,
            translations:
            inputTranslations);

        Dictionary<string, object> result = [];

        int inputTokens = 0;
        int outputTokens = 0;

        foreach (var prompt in prompts)
        {
            promptCount++;

            Console.WriteLine($" -> Request #{promptCount}");

            var response = await ChatGptRepository.DoRequest(
                model: gptConfig.Model,
                temperature: gptConfig.Temperature,
                apiKey: apiKey,
                prompt: prompt
            );

            bool hasError = response == null;

            if (debug || hasError)
            {
                if (hasError)
                    Console.WriteLine(" -> Error while parsing JSON. Writing to log file.");

                Logger.LogGptRequest(
                    fromLocale: fileCollection.Config.BaseLocale,
                    toLocale: targetLocale,
                    fromFile: file.Path,
                    toFile: targetPath,
                    outDir: outDir,
                    promptCount: promptCount,
                    prompt: prompt,
                    response: response
                );
            }

            if (!hasError)
            {
                result = Apply.applyMapRecursive(
                    baseMap: originalTranslations,
                    newMap: response!.JsonMessage,
                    oldMap: result,
                    verbose: false
                );
            }

            inputTokens += response!.PromptTokens;
            outputTokens += response.CompletionTokens;
        }

        // add existing translations
        result = Apply.applyMapRecursive(
            baseMap: originalTranslations,
            newMap: existingTranslations,
            oldMap: result,
            verbose: false
        );

        // add comments from base locale to target locale
        result = Apply.applyMapRecursive(
            baseMap: originalTranslations,
            newMap: comments,
            oldMap: result,
            verbose: false
        );

        FileUtils.WriteFileOfType(
            path: targetPath,
            content: result
        );

        Console.WriteLine($" -> Output: {targetPath}");

        return new TranslateMetrics(
            EndPromptCount: promptCount,
            InputTokens: inputTokens,
            OutputTokens: outputTokens
        );
    }
}