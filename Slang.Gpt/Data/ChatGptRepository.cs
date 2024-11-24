using System.Text;
using System.Text.Json;
using Serilog;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Prompt;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Data;

internal class ChatGptRepository(ILogger logger, HttpClient httpClient, string apiUrl)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

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
            _ => throw new NotImplementedException(),
        };
    }

    private async Task<GptResponse?> DoRequestForOpenAi(
        GptModel.GptModelInfo model,
        double? temperature,
        GptPrompt prompt)
    {
        string jsonRequestBody = CreateRequestBody(model, prompt, temperature);

        var response = await httpClient.PostAsync(apiUrl,
            new StringContent(jsonRequestBody, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            logger.Information(
                $"{nameof(ChatGptRepository)}.{nameof(DoRequestForOpenAi)} - {nameof(jsonRequestBody)}:{jsonRequestBody}");
        }

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        var rawMap = JsonSerializer.Deserialize(responseBody, GptResponseDtoContext.Default.GptResponseDto);

        if (rawMap?.Choices == null || rawMap.Choices.Count < 1)
            return null;

        string? rawMessage = rawMap.Choices[0].Message?.Content;

        if (rawMessage == null)
            return null;

        var jsonMessage = GetLocales(rawMessage);

        if (jsonMessage == null)
            return null;

        return new GptResponse(
            RawMessage: rawMessage,
            JsonMessage: jsonMessage,
            PromptTokens: rawMap.Usage?.PromptTokens ?? 0,
            CompletionTokens: rawMap.Usage?.CompletionTokens ?? 0,
            TotalTokens: rawMap.Usage?.TotalTokens ?? 0
        );
    }

    private static string CreateRequestBody(GptModel.GptModelInfo model, GptPrompt prompt, double? temperature)
    {
        string systemContent = prompt.System.Replace(Environment.NewLine, "\n");
        string userContent = prompt.User.Replace(Environment.NewLine, "\n");

        var messages = new[]
        {
            new { role = "system", content = systemContent },
            new { role = "user", content = userContent }
        };

        var requestBody = new
        {
            model = model.Id,
            temperature,
            messages
        };

        return JsonSerializer.Serialize(requestBody, JsonOptions);
    }

    private static Dictionary<string, object?>? GetLocales(string rawMessage)
    {
        try
        {
            return JsonHelpers.JsonDecode(rawMessage.StartsWith('{')
                ? rawMessage
                // catching case: rawMessage starts with ```json
                : string.Join(Environment.NewLine, rawMessage.Split('\n').Skip(1).SkipLast(1)));
        }
        catch (Exception)
        {
            // ignored
        }

        return null;
    }
}