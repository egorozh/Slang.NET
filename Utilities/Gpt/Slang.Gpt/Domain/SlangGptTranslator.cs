using System.Globalization;
using Serilog;
using Slang.Gpt.Data;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Prompt;
using Slang.Gpt.Domain.Utils;
using Slang.Shared;
using Slang.Utilities.Core.Translate;

namespace Slang.Gpt.Domain;

internal record TranslateMetrics(
    int EndPromptCount,
    int InputTokens,
    int OutputTokens,
    string? TargetPath);

internal record NoNewTranslationsMetrics(int EndPromptCount) : TranslateMetrics(EndPromptCount, 0, 0, null);

public delegate void PartialTranslationHandler(string filePath);
public delegate void StartRequestHandler(int promptCount);

internal sealed class SlangGptTranslator(
    ILogger logger,
    ChatGptRepository chatGptRepository,
    List<TranslationFile> files,
    GptConfig gptConfig,
    bool full,
    PartialTranslationHandler partialTranslationHandler,
    StartRequestHandler startRequestHandler)
{
    /// <summary>
    /// Translates a file to a target locale.
    /// </summary>
    public async Task<TranslateMetrics> Translate(
        CultureInfo targetLocale,
        string outDir,
        TranslationFile file,
        Dictionary<string, object?> originalTranslations,
        int promptCount
    )
    {
        // existing translations of target locale
        Dictionary<string, object?> existingTranslations = [];

        string targetPath = Path.Combine(
            outDir,
            $"{file.Namespace}_{targetLocale}{Constants.AdditionalFilePattern}"
        );

        foreach (var destFile in files)
        {
            if (destFile.Namespace == file.Namespace
                && Equals(destFile.Locale.TwoLetterISOLanguageName, targetLocale.TwoLetterISOLanguageName))
            {
                targetPath = destFile.FilePath;

                if (full)
                    break;

                string raw = await destFile.Read();

                try
                {
                    existingTranslations = JsonHelpers.JsonDecode(raw);

                    partialTranslationHandler(destFile.FilePath);
                }
                catch (Exception e)
                {
                    throw new Exception($"File: {destFile.FilePath}{Environment.NewLine}{e}");
                }

                break;
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
            return new NoNewTranslationsMetrics(EndPromptCount: promptCount);

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

            startRequestHandler(promptCount);

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

        return new TranslateMetrics(
            EndPromptCount: promptCount,
            InputTokens: inputTokens,
            OutputTokens: outputTokens,
            targetPath
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