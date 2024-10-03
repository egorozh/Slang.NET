using System.Globalization;
using Slang.Gpt.Data;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Utils;
using Slang.Shared;

namespace Slang.Gpt.Domain;

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
        HttpClient httpClient,
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        CultureInfo targetLocale,
        string outDir,
        bool full,
        bool debug,
        TranslationFile file,
        Dictionary<string, object?> originalTranslations,
        int promptCount
    )
    {
        Console.WriteLine("");
        Console.WriteLine(
            $"Translating <{file.Locale.TwoLetterISOLanguageName}> to <{targetLocale.TwoLetterISOLanguageName}> for {file.FileName} ...");

        // existing translations of target locale
        Dictionary<string, object?> existingTranslations = [];

        string targetPath = Path.Combine(
            outDir,
            $"{file.Namespace}_{targetLocale.TwoLetterISOLanguageName}{Constants.AdditionalFilePattern}"
        );

        if (!full)
        {
            foreach (var destFile in fileCollection.Files)
            {
                if (destFile.Namespace == file.Namespace && Equals(destFile.Locale, targetLocale))
                {
                    string raw = await destFile.Read();

                    try
                    {
                        existingTranslations = JsonHelpers.JsonDecode(raw);
                        targetPath = destFile.FilePath!;
                        Console.WriteLine($" -> With partial translations from {destFile.FilePath!}");
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"File: {destFile.FilePath!}\n{e}");
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
        Maps.RemoveIgnoreGpt(map: inputTranslations);

        // extract original comments but keep them in the inputTranslations
        // we will add the original comments later again
        var comments = Maps.ExtractComments(
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
            targetCulture: targetLocale,
            config: gptConfig,
            translations:
            inputTranslations);

        Dictionary<string, object?> result = [];

        int inputTokens = 0;
        int outputTokens = 0;

        foreach (var prompt in prompts)
        {
            promptCount++;

            Console.WriteLine($" -> Request #{promptCount}");

            var response = await ChatGptRepository.DoRequest(
                model: gptConfig.Model,
                httpClient: httpClient,
                temperature: gptConfig.Temperature,
                prompt: prompt
            );

            bool hasError = response == null;

            if (debug || hasError)
            {
                if (hasError)
                    Console.WriteLine(" -> Error while parsing JSON. Writing to log file.");

                Logger.LogGptRequest(
                    fromLocale: gptConfig.BaseCulture,
                    toLocale: targetLocale,
                    fromFile: file.Locale.Name,
                    toFile: targetPath,
                    outDir: outDir,
                    promptCount: promptCount,
                    prompt: prompt,
                    response: response
                );
            }

            if (!hasError)
            {
                result = MapUtils.ApplyMapRecursive(
                    baseMap: originalTranslations,
                    newMap: response!.JsonMessage,
                    oldMap: result,
                    verbose: false
                );
            }

            inputTokens += response.PromptTokens;
            outputTokens += response.CompletionTokens;
        }

        // add existing translations
        result = MapUtils.ApplyMapRecursive(
            baseMap: originalTranslations,
            newMap: existingTranslations,
            oldMap: result,
            verbose: false
        );

        // add comments from base locale to target locale
        result = MapUtils.ApplyMapRecursive(
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