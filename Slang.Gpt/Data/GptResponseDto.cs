namespace Slang.Gpt.Data;

internal record GptResponseDto(
    List<ChoiseDto>? choises,
    UsageDto usage
);

internal record ChoiseDto(MessageDto message);

internal record MessageDto(string content);

internal record UsageDto(
    int prompt_tokens,
    int completion_tokens,
    int total_tokens
);