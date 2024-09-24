namespace Slang.Gpt.Models;

/// <summary>
/// The prompt that will be sent to the GPT API.
/// </summary>
/// <param name="System">Contains the general instruction and the app description.</param>
/// <param name="User">Contains the base translations.</param>
/// <param name="UserJson">Contains the JSON representation of the prompt. (Debugging)</param>
public record GptPrompt(
    string System,
    string User,
    Dictionary<string, object> UserJson);