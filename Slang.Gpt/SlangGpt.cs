using System.Globalization;
using Slang.Gpt.Data;
using Slang.Gpt.Domain;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt;

public static class SlangGpt
{
    public static async Task Execute(
        HttpClient httpClient,
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        List<CultureInfo>? targetLocales = null,
        bool debug = false,
        bool full = false)
    {
        Console.WriteLine(
            $"GPT config: {gptConfig.Model.Id} / {gptConfig.MaxInputLength} max input length / {gptConfig.Temperature?.ToString() ?? "default"} temperature"
        );

        if (gptConfig.Excludes.Count > 0)
        {
            Console.WriteLine(
                $"Excludes: {string.Join(", ", gptConfig.Excludes.Select(e => e.EnglishName))}");
        }

        int promptCount = 0;
        int inputTokens = 0;
        int outputTokens = 0;

        foreach (var file in fileCollection.Files)
        {
            string outDir = new FileInfo(file.FilePath!).Directory!.FullName;

            if (!Equals(file.Locale, gptConfig.BaseCulture))
            {
                // Only use base locale as source
                continue;
            }

            string raw = await file.Read();

            var targetLocalesEnumerable = targetLocales ?? GetExistingLocales(fileCollection, gptConfig, file);

            foreach (var targetLocale in targetLocalesEnumerable)
            {
                var metrics = await SlangGptTranslator.Translate(
                    httpClient: httpClient,
                    fileCollection: fileCollection,
                    gptConfig: gptConfig,
                    targetLocale: targetLocale,
                    outDir: outDir,
                    full: full,
                    debug: debug,
                    file: file,
                    originalTranslations: GetOriginalTranslations(raw, file),
                    promptCount: promptCount
                );

                promptCount = metrics.EndPromptCount;
                inputTokens += metrics.InputTokens;
                outputTokens += metrics.OutputTokens;
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Summary:");
        Console.WriteLine($" -> Total requests: {promptCount}");
        Console.WriteLine($" -> Total input tokens: {inputTokens}");
        Console.WriteLine($" -> Total output tokens: {outputTokens}");

        double totalCost = inputTokens * gptConfig.Model.CostPerInputToken +
                           outputTokens * gptConfig.Model.CostPerOutputToken;

        Console.WriteLine(
            $" -> Total cost: {totalCost:C} ({inputTokens} x ${gptConfig.Model.CostPerInputToken} + {outputTokens} x {gptConfig.Model.CostPerOutputToken})");
    }

    private static IEnumerable<CultureInfo> GetExistingLocales(
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        TranslationFile file)
    {
        foreach (var destFile in fileCollection.Files)
        {
            if (gptConfig.Excludes.Contains(destFile.Locale))
            {
                // skip excluded locales
                continue;
            }

            if (Equals(destFile.Locale, file.Locale))
            {
                // skip same locale
                continue;
            }

            yield return destFile.Locale;
        }
    }


    private static Dictionary<string, object?> GetOriginalTranslations(string raw, TranslationFile file)
    {
        Dictionary<string, object?> originalTranslations;

        try
        {
            originalTranslations = JsonHelpers.JsonDecode(raw);
        }
        catch (Exception e)
        {
            throw new Exception($"File: ${file.FileName}\n{e}");
        }

        return originalTranslations;
    }
}