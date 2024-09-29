using System.Text.Json.Serialization;

namespace Slang.Gpt.Data;

internal record GptResponseDto(
    List<ChoiseDto>? choices,
    UsageDto usage
);

internal record ChoiseDto(MessageDto message);

internal record MessageDto(string content);

internal record UsageDto(
    int prompt_tokens,
    int completion_tokens,
    int total_tokens
);

[JsonSerializable(typeof(GptResponseDto))]
[JsonSerializable(typeof(ChoiseDto))]
[JsonSerializable(typeof(MessageDto))]
[JsonSerializable(typeof(UsageDto))]
internal partial class GptResponseDtoContext : JsonSerializerContext;