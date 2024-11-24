using System.Text.Json.Serialization;

namespace Slang.Gpt.Data;

internal record GptResponseDto
{
    [JsonPropertyName("choices")] public List<ChoiceDto>? Choices { get; set; }

    [JsonPropertyName("usage")] public UsageDto? Usage { get; set; }
}

internal record ChoiceDto
{
    [JsonPropertyName("message")] public MessageDto? Message { get; set; }
}

internal record MessageDto
{
    [JsonPropertyName("content")] public string? Content { get; set; }
}

internal record UsageDto
{
    [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
}

[JsonSerializable(typeof(GptResponseDto))]
[JsonSerializable(typeof(ChoiceDto))]
[JsonSerializable(typeof(MessageDto))]
[JsonSerializable(typeof(UsageDto))]
internal partial class GptResponseDtoContext : JsonSerializerContext;