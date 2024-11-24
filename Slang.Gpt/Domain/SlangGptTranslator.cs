using System.Globalization;
using Serilog;
using Slang.Gpt.Data;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Prompt;
using Slang.Gpt.Domain.Utils;
using Slang.Shared;

namespace Slang.Gpt.Domain;

internal record TranslateMetrics(
    int EndPromptCount,
    int InputTokens,
    int OutputTokens);

internal sealed class SlangGptTranslator(ILogger logger, ChatGptRepository chatGptRepository)
{
    /// <summary>
    /// Translates a file to a target locale.
    /// </summary>
    public async Task<TranslateMetrics> Translate(
        SlangFileCollection fileCollection,
        GptConfig gptConfig,
        CultureInfo targetLocale,
        string outDir,
        bool full,
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

            var response = await chatGptRepository.DoRequest(
                model: gptConfig.Model,
                temperature: gptConfig.Temperature,
                prompt: prompt
            );

            bool hasError = response == null;

            if (hasError)
            {
                if (hasError)
                    Console.WriteLine(" -> Error while parsing JSON. Writing to log file.");

                string log = GetGptRequestLogMessage(
                    fromLocale: gptConfig.BaseCulture,
                    toLocale: targetLocale,
                    fromFile: file.Locale.Name,
                    toFile: targetPath,
                    prompt: prompt,
                    response: response
                );

                logger.Error(log);
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

            inputTokens += response?.PromptTokens ?? 0;
            outputTokens += response?.CompletionTokens ?? 0;
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

    /// Logs the GPT request and response to a file.
    private static string GetGptRequestLogMessage(
        CultureInfo fromLocale,
        CultureInfo toLocale,
        string fromFile,
        string toFile,
        GptPrompt prompt,
        GptResponse? response
    )
    {
        return $"""
                ### Meta ###
                From: <{fromLocale.TwoLetterISOLanguageName}> {fromFile}
                To: <{toLocale.TwoLetterISOLanguageName}> {toFile}

                ### Tokens ###
                Input: {response?.PromptTokens}
                Output: {response?.CompletionTokens}
                Total: {response?.TotalTokens}

                ### Conversation ###

                >> System:
                {prompt.System}

                >> User:
                {prompt.User}

                >> Assistant:
                {response?.RawMessage}

                ### JSON ###
                Input:
                {JsonHelpers.JsonEncode(prompt.UserJson)}

                Output:
                {JsonHelpers.JsonEncode(response?.JsonMessage ?? [])}
                """;
    }
}