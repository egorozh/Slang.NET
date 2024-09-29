using System.Globalization;
using Slang.Gpt.Models;
using Slang.Gpt.Prompt;

namespace Slang.Gpt.Utils;

internal static class Logger
{
    /// Logs the GPT request and response to a file.
    public static void LogGptRequest(
        CultureInfo fromLocale,
        CultureInfo toLocale,
        string fromFile,
        string toFile,
        string outDir,
        int promptCount,
        GptPrompt prompt,
        GptResponse? response
    )
    {
        string path = Path.Combine(
            outDir,
            $"_gpt_{promptCount.ToString().PadLeft(2, '0')}.txt"
        );

        File.WriteAllText(
            path: path,
            $"""
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
             {JsonHelpers.JsonEncode(response?.JsonMessage)}
             """);

        Console.WriteLine($" -> Logs: {path}");
    }
}