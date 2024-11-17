using System.Text;
using System.Text.Json;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Prompt;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Data;

internal static class ChatGptRepository
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Sends a prompt to a GPT provider and returns the response.
    /// </summary>
    public static Task<GptResponse?> DoRequest(
        GptModel.GptModelInfo model,
        HttpClient httpClient,
        double? temperature,
        GptPrompt prompt)
    {
        return model.Provider switch
        {
            GptProvider.OpenAi => DoRequestForOpenAi(model, httpClient, temperature, prompt),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static async Task<GptResponse?> DoRequestForOpenAi(
        GptModel.GptModelInfo model,
        HttpClient httpClient,
        double? temperature,
        GptPrompt prompt)
    {
        string systemContent = prompt.System.Replace(Environment.NewLine, "\\n");
        string userContent = prompt.User.Replace(Environment.NewLine, "\\n").Replace("\"", "\\\"");
        
        string jsonRequestBody = temperature != null
            ? $$"""
                {
                    "model": "{{model.Id}}",
                    "temperature": {{temperature}},
                    "messages":
                    [
                        {
                            "role": "system",
                            "content": "{{systemContent}}"
                        },
                        {
                            "role": "user",
                            "content": "{{userContent}}"
                        }
                    ]
                }
                """
            : $$"""
                {
                    "model": "{{model.Id}}",
                    "messages":
                    [
                        {
                            "role": "system",
                            "content": "{{systemContent}}"
                        },
                        {
                            "role": "user",
                            "content": "{{userContent}}"
                        }
                    ]
                }
                """;

        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiUrl, content);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        var rawMap = JsonSerializer.Deserialize(responseBody, GptResponseDtoContext.Default.GptResponseDto);

        if (rawMap?.choices == null || rawMap.choices.Count == 0)
            return null;

        string rawMessage = rawMap.choices[0].message.content;

        Dictionary<string, object?>? jsonMessage = null;

        try
        {
            jsonMessage = JsonHelpers.JsonDecode(rawMessage.StartsWith('{')
                ? rawMessage
                // catching case: rawMessage starts with ```json
                : string.Join(Environment.NewLine, rawMessage.Split('\n').Skip(1).SkipLast(1)));
        }
        catch (Exception)
        {
            // ignored
        }

        if (jsonMessage == null)
            return null;

        return new GptResponse(
            RawMessage: rawMessage,
            JsonMessage: jsonMessage,
            PromptTokens: rawMap.usage.prompt_tokens,
            CompletionTokens: rawMap.usage.completion_tokens,
            TotalTokens: rawMap.usage.total_tokens
        );
    }
}