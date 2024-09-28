using System.Globalization;

namespace Slang.Gpt.Models;

/// <summary>
/// Represents the gpt node in build.yaml
/// </summary>
/// <param name="Model">The GPT model that should be used.</param>
/// <param name="Description">
/// The app description that will be part of the "system" prompt.
/// Usually, this provides the GPT model with some context for more
/// accurate results.
/// </param>
/// <param name="MaxInputLength">
/// The maximum amount of characters that can be sent to the GPT API
/// in one request. Lower values will result in more requests.
/// </param>
/// <param name="Temperature">The temperature parameter for the GPT API (if supported).</param>
/// <param name="Excludes">List of excluded target locales.</param>
public record GptConfig(
    CultureInfo BaseCulture,
    GptModel.GptModelInfo Model,
    string Description,
    int MaxInputLength,
    double? Temperature,
    List<CultureInfo> Excludes);