namespace Slang.Gpt.Domain.Models;

/// <summary>
/// A response from the GPT API.
/// </summary>
/// <param name="RawMessage">The raw prompt answer.</param>
/// <param name="JsonMessage">The parsed prompt answer.</param>
/// <param name="PromptTokens">The number of input tokens.</param>
/// <param name="CompletionTokens">The number of output tokens.</param>
/// <param name="TotalTokens">The total number of tokens.</param>
public record GptResponse(
    string RawMessage,
    Dictionary<string, object?> JsonMessage,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);