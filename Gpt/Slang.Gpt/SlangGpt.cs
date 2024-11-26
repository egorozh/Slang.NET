using System.Globalization;
using Serilog;
using Slang.Gpt.Data;
using Slang.Gpt.Domain;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt;

public static class SlangGpt
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public static async Task Execute(
        ILogger logger,
        HttpClient httpClient,
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        List<CultureInfo>? targetLocales = null,
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

        ChatGptRepository chatGptRepository = new(logger, httpClient, ApiUrl);
        SlangGptTranslator slangGptTranslator = new(logger, chatGptRepository);

        foreach (var file in fileCollection.Files)
        {
            string outDir = new FileInfo(file.FilePath).Directory!.FullName;

            if (!Equals(file.Locale, gptConfig.BaseCulture))
            {
                // Only use base locale as source
                continue;
            }

            string fileContent = await file.Read();
            var fileLocales = GetLocales(fileContent, file.FileName);

            var targetLocalesEnumerable = targetLocales ?? GetExistingLocales(fileCollection, gptConfig, file);

            foreach (var targetLocale in targetLocalesEnumerable)
            {
                var metrics = await slangGptTranslator.Translate(
                    files: fileCollection.Files,
                    gptConfig: gptConfig,
                    targetLocale: targetLocale,
                    outDir: outDir,
                    full: full,
                    file: file,
                    originalTranslations: fileLocales,
                    promptCount: promptCount
                );

                promptCount = metrics.EndPromptCount;
                inputTokens += metrics.InputTokens;
                outputTokens += metrics.OutputTokens;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($" -> Total requests: {promptCount}");
        Console.WriteLine($" -> Total input tokens: {inputTokens}");
        Console.WriteLine($" -> Total output tokens: {outputTokens}");

        double totalCost = inputTokens * gptConfig.Model.CostPerInputToken +
                           outputTokens * gptConfig.Model.CostPerOutputToken;

        CultureInfo dollarCulture = new("en-US");

        string totalCostString = totalCost.ToString("C6", dollarCulture);
        string perInputTokenCostString = gptConfig.Model.CostPerInputToken.ToString("C8", dollarCulture);
        string perOutputTokenCostString = gptConfig.Model.CostPerOutputToken.ToString("C8", dollarCulture);

        Console.WriteLine(
            $" -> Total cost: {totalCostString} ({inputTokens} x {perInputTokenCostString} + {outputTokens} x {perOutputTokenCostString})");
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


    private static Dictionary<string, object?> GetLocales(string fileContent, string fileName)
    {
        Dictionary<string, object?> originalTranslations;

        try
        {
            originalTranslations = JsonHelpers.JsonDecode(fileContent);
        }
        catch (Exception e)
        {
            throw new Exception($"File: {fileName}{Environment.NewLine}{e}");
        }

        return originalTranslations;
    }
}