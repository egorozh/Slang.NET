using System.Globalization;
using System.Text.Json;
using Slang.Gpt.Models;
using Slang.Gpt.Prompt;

namespace Slang.Gpt.Utils;

internal static class Logger
{
    //static JsonEncoder _encoder = JsonEncoder.withIndent("  ");
    private static readonly JsonSerializerOptions Options = new(
        JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static string Encoder(object? obj) => JsonSerializer.Serialize(obj, Options);

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
        string path = PathUtils.WithFileName(
            directoryPath: outDir,
            fileName: $"_gpt_{promptCount.ToString().PadLeft(2, '0')}.txt",
            pathSeparator: Path.PathSeparator
        );

        FileUtils.writeFile(
            path: path,
            content:
            $"""
             ### Meta ###
             From: <{fromLocale.TwoLetterISOLanguageName}> {fromFile}
             To: <{toLocale.TwoLetterISOLanguageName}> {toFile}

             ### Tokens ###
             Input: {response.PromptTokens}
             Output: {response.CompletionTokens}
             Total: {response.TotalTokens}

             ### Conversation ###

             >> System:
             {prompt.System}

             >> User:
             {prompt.User}

             >> Assistant:
             {response.RawMessage}

             ### JSON ###
             Input:
             {Encoder(prompt.UserJson)}

             Output:
             {Encoder(response?.JsonMessage)}
             """);

        Console.WriteLine($" -> Logs: {path}");
    }
}