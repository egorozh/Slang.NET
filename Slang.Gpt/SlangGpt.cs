using System.Globalization;
using Slang.Gpt.Data;
using Slang.Gpt.Models;

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

            Dictionary<string, object?> originalTranslations;

            try
            {
                originalTranslations = JsonHelpers.JsonDecode(raw);
            }
            catch (Exception e)
            {
                throw new Exception($"File: ${file.FileName}\n{e}");
            }

            if (targetLocales == null)
            {
                // translate to existing locales
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

                    var metrics = await SlangGptTranslator.Translate(
                        httpClient: httpClient,
                        fileCollection: fileCollection,
                        gptConfig: gptConfig,
                        targetLocale: destFile.Locale,
                        outDir: outDir,
                        full: full,
                        debug: debug,
                        file: file,
                        originalTranslations: originalTranslations,
                        promptCount: promptCount
                    );

                    promptCount = metrics.EndPromptCount;
                    inputTokens += metrics.InputTokens;
                    outputTokens += metrics.OutputTokens;
                }
            }
            else
            {
                // translate to specified locales (they may not exist yet)
                foreach (var targetLocale in targetLocales)
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
                        originalTranslations: originalTranslations,
                        promptCount: promptCount
                    );

                    promptCount = metrics.EndPromptCount;
                    inputTokens += metrics.InputTokens;
                    outputTokens += metrics.OutputTokens;
                }
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Summary:");
        Console.WriteLine($" -> Total requests: {promptCount}");
        Console.WriteLine($" -> Total input tokens: {inputTokens}");
        Console.WriteLine($" -> Total output tokens: {outputTokens}");
        Console.WriteLine(
            $" -> Total cost: ${inputTokens * gptConfig.Model.CostPerInputToken + outputTokens * gptConfig.Model.CostPerOutputToken} ($inputTokens x ${gptConfig.Model.CostPerInputToken} + $outputTokens x ${gptConfig.Model.CostPerOutputToken})");
    }
}