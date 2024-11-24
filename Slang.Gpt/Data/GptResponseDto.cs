using System.Text.Json.Serialization;

namespace Slang.Gpt.Data;

internal record GptResponseDto(
    [property: JsonPropertyName("choices")]
    List<ChoiseDto>? Choices,
    [property: JsonPropertyName("usage")] UsageDto Usage
);

internal record ChoiseDto(
    [property: JsonPropertyName("message")]
    MessageDto Message);

internal record MessageDto(
    [property: JsonPropertyName("content")]
    string Content);

internal record UsageDto(
    [property: JsonPropertyName("prompt_tokens")]
    int PromptTokens,
    [property: JsonPropertyName("completion_tokens")]
    int CompletionTokens,
    [property: JsonPropertyName("total_tokens")]
    int TotalTokens
);

#if(NET7_0_OR_GREATER)
[JsonSerializable(typeof(GptResponseDto))]
[JsonSerializable(typeof(ChoiseDto))]
[JsonSerializable(typeof(MessageDto))]
[JsonSerializable(typeof(UsageDto))]
internal partial class GptResponseDtoContext : JsonSerializerContext;
#endif