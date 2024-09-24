using System.Text;
using System.Text.Json;
using Slang.Gpt.Models;

namespace Slang.Gpt.Data;

internal static class ChatGptRepository
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// Sends a prompt to a GPT provider and returns the response.
    /// </summary>
    public static Task<GptResponse?> DoRequest(
        GptModel.GptModelInfo model,
        double? temperature,
        string apiKey,
        GptPrompt prompt)
    {
        return model.Provider switch
        {
            GptProvider.OpenAi => DoRequestForOpenAi(model, temperature, apiKey, prompt),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static async Task<GptResponse?> DoRequestForOpenAi(
        GptModel.GptModelInfo model,
        double? temperature,
        string apiKey,
        GptPrompt prompt)
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        string jsonRequestBody = temperature != null
            ? JsonSerializer.Serialize(new
            {
                model = model.Id,
                temperature,
                messages = new[]
                {
                    new {role = "system", content = prompt.System},
                    new {role = "user", content = prompt.User}
                }
            })
            : JsonSerializer.Serialize(new
            {
                model = model.Id,
                messages = new[]
                {
                    new {role = "system", content = prompt.System},
                    new {role = "user", content = prompt.User}
                }
            });
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiUrl, content);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        var rawMap = JsonSerializer.Deserialize<GptResponseDto>(responseBody);

        if (rawMap?.choises == null || rawMap.choises.Count == 0)
            return null;

        string rawMessage = rawMap.choises[0].message.content;

        Dictionary<string, object>? jsonMessage = null;

        try
        {
            jsonMessage = JsonSerializer.Deserialize<Dictionary<string, object>>(rawMessage);
        }
        catch (Exception _)
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