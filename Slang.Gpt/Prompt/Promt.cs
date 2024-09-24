using System.Globalization;
using Slang.Generator.Config.Domain;
using Slang.Generator.Config.Domain.Entities;
using Slang.Gpt.Models;
using Slang.Gpt.Utils;
using static Slang.Gpt.JsonHelpers;

namespace Slang.Gpt.Prompt;

internal static class Prompt
{
    /// <summary>
    /// Returns the prompts that should be sent to GPT.
    /// There can be multiple prompts if the input is too long.
    /// </summary>
    public static List<GptPrompt> GetPrompts(
        RawConfig rawConfig,
        CultureInfo targetLocale,
        GptConfig config,
        string? namespaceStroke,
        Dictionary<string, dynamic> translations
    )
    {
        string systemPrompt = _getSystemPrompt(
            rawConfig: rawConfig,
            targetLocale: targetLocale,
            config: config,
            namespaceStroke: namespaceStroke
        );

        int systemPromptLength = systemPrompt.Length;

        List<GptPrompt> prompts = [];
        Dictionary<string, object> currentTranslationWindow = [];

        foreach (var entry in translations)
        {
            if (currentTranslationWindow.Count == 0)
            {
                currentTranslationWindow[entry.Key] = entry.Value;
                continue;
            }

            string currentTranslation = jsonEncode(currentTranslationWindow);
            currentTranslationWindow[entry.Key] = entry.Value;
            string nextTranslation = jsonEncode(currentTranslationWindow);

            if (systemPromptLength + nextTranslation.Length > config.MaxInputLength)
            {
                prompts.Add(new GptPrompt(
                    System: systemPrompt,
                    User: currentTranslation,
                    // currentTranslationWindow has been changed already
                    UserJson: jsonDecode(currentTranslation)
                ));

                currentTranslationWindow = new Dictionary<string, object>
                {
                    {entry.Key, entry.Value}
                };
            }
        }

        if (currentTranslationWindow.Count > 0)
        {
            // add the last prompt
            prompts.Add(new GptPrompt(
                System: systemPrompt,
                User: jsonEncode(currentTranslationWindow),
                UserJson: currentTranslationWindow
            ));
        }

        return prompts;
    }

    private static string _getSystemPrompt(
        RawConfig rawConfig,
        CultureInfo targetLocale,
        GptConfig config,
        string? namespaceStroke
    )
    {
        const string interpolationHint = "{parameter}";

        string namespaceHint = namespaceStroke != null ? $" the \"{namespaceStroke}\" part of" : "";

        return $"""
                The user wants to internationalize{namespaceHint} the app. The user will provide you with a JSON file containing the {rawConfig.BaseLocale.EnglishName} strings.
                You will translate it to {targetLocale.EnglishName}.
                Parameters are interpolated with {interpolationHint}.
                Linked translations are denoted with the \"@:path0.path1\" syntax.

                Here is the app description. Respect this context when translating:
                {config.Description}
                """
            ;
    }
}