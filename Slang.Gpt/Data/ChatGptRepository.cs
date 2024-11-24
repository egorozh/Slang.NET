using System.Text;
using System.Text.Json;
using Serilog;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Prompt;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Data;

internal class ChatGptRepository(ILogger logger, HttpClient httpClient, string apiUrl)
{
    /// <summary>
    /// Sends a prompt to a GPT provider and returns the response.
    /// </summary>
    public Task<GptResponse?> DoRequest(
        GptModel.GptModelInfo model,
        double? temperature,
        GptPrompt prompt)
    {
        return model.Provider switch
        {
            GptProvider.OpenAi => DoRequestForOpenAi(model, temperature, prompt),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task<GptResponse?> DoRequestForOpenAi(
        GptModel.GptModelInfo model,
        double? temperature,
        GptPrompt prompt)
    {
        string systemContent = prompt.System
            .Replace(Environment.NewLine, "\n")
            .Replace("\n", "\\n");
        string userContent = prompt.User
            .Replace(Environment.NewLine, "\n")
            .Replace("\n", "\\n")
            .Replace("\"", "\\\"");

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

        var response = await httpClient.PostAsync(apiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            logger.Information(
                $"{nameof(ChatGptRepository)}.{nameof(DoRequestForOpenAi)} - {nameof(jsonRequestBody)}:{jsonRequestBody}");
        }

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        
        var rawMap = JsonSerializer.Deserialize(responseBody, GptResponseDtoContext.Default.GptResponseDto);
        
        if (rawMap?.Choices == null || rawMap.Choices.Count == 0)
            return null;

        string rawMessage = rawMap.Choices[0].Message.Content;

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
            PromptTokens: rawMap.Usage.PromptTokens,
            CompletionTokens: rawMap.Usage.CompletionTokens,
            TotalTokens: rawMap.Usage.TotalTokens
        );
    }
}