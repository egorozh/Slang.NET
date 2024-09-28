using System.Globalization;
using Slang.Gpt.Models;

using static Slang.Gpt.JsonHelpers;

namespace Slang.Gpt.Prompt;

/// <summary>
/// The prompt that will be sent to the GPT API.
/// </summary>
/// <param name="System">Contains the general instruction and the app description.</param>
/// <param name="User">Contains the base translations.</param>
/// <param name="UserJson">Contains the JSON representation of the prompt. (Debugging)</param>
internal record GptPrompt(
    string System,
    string User,
    Dictionary<string, object> UserJson);

internal static class Prompt
{
    /// <summary>
    /// Returns the prompts that should be sent to GPT.
    /// There can be multiple prompts if the input is too long.
    /// </summary>
    public static List<GptPrompt> GetPrompts(
        CultureInfo targetCulture,
        GptConfig config,
        Dictionary<string, object?> translations
    )
    {
        string systemPrompt = GetSystemPrompt(
            baseCulture: config.BaseCulture,
            targetCulture: targetCulture,
            config: config
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

    private static string GetSystemPrompt(
        CultureInfo baseCulture,
        CultureInfo targetCulture,
        GptConfig config
    )
    {
        return $$"""
                 The user wants to internationalize the app. The user will provide you with a JSON file containing the {{baseCulture.EnglishName}} strings.
                 You will translate it to {{targetCulture.EnglishName}}.
                 Parameters are interpolated with {parameter}.
                 Linked translations are denoted with the \"@:path0.path1\" syntax.

                 Here is the app description. Respect this context when translating:
                 {{config.Description}}
                 """;
    }
}