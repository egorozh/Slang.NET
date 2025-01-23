using System.Globalization;
using Serilog;
using Slang.Gpt.Data;
using Slang.Gpt.Domain;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Utils;
using Slang.Utilities.Core.Translate;

namespace Slang.Gpt;

public record struct GptResult(
    int PromptCount,
    int InputTokens,
    int OutputTokens
);

public delegate void StartTranslateHandler(TranslationFile file, CultureInfo targetCulture);

public delegate void NoNewTranslationsHandler();

public delegate void EndTranslateHandler(string? targetPath);

public static class SlangGpt
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public static Task<GptResult> Execute(
        ILogger logger,
        HttpClient httpClient,
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        StartTranslateHandler startTranslateHandler,
        PartialTranslationHandler partialTranslationHandler,
        StartRequestHandler startRequestHandler,
        NoNewTranslationsHandler noNewTranslationsHandler,
        EndTranslateHandler endTranslateHandler,
        List<CultureInfo>? targetLocales = null,
        bool full = false)
    {
        ChatGptRepository chatGptRepository = new(logger, httpClient, ApiUrl);
        SlangGptTranslator slangGptTranslator = new(
            logger,
            chatGptRepository,
            fileCollection.Files,
            gptConfig,
            full,
            partialTranslationHandler,
            startRequestHandler);

        return ExecuteImpl(
            gptConfig,
            fileCollection,
            targetLocales,
            slangGptTranslator,
            startTranslateHandler,
            noNewTranslationsHandler,
            endTranslateHandler);
    }


    private static async Task<GptResult> ExecuteImpl(
        GptConfig gptConfig,
        SlangFileCollection fileCollection,
        List<CultureInfo>? targetLocales,
        SlangGptTranslator slangGptTranslator,
        StartTranslateHandler startTranslateHandler,
        NoNewTranslationsHandler noNewTranslationsHandler,
        EndTranslateHandler endTranslateHandler)
    {
        int promptCount = 0;
        int inputTokens = 0;
        int outputTokens = 0;

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
                startTranslateHandler(file, targetLocale);

                var metrics = await slangGptTranslator.Translate(
                    targetLocale: targetLocale,
                    outDir: outDir,
                    file: file,
                    originalTranslations: fileLocales,
                    promptCount: promptCount
                );

                if (metrics is NoNewTranslationsMetrics)
                    noNewTranslationsHandler();
                else
                    endTranslateHandler(metrics.TargetPath);
                
                promptCount = metrics.EndPromptCount;
                inputTokens += metrics.InputTokens;
                outputTokens += metrics.OutputTokens;
            }
        }

        return new GptResult(promptCount, inputTokens, outputTokens);
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