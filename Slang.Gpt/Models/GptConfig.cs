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
    GptModel.GptModelInfo Model,
    string Description,
    int MaxInputLength,
    double? Temperature,
    List<CultureInfo> Excludes)
{
    // static GptConfig FromMap(Dictionary<string, object> map)
    // {

    // final Map<String, dynamic>?
    // gpt = map['gpt'];
    //
    // if (gpt == null)
    // {
    //     throw 'Missing gpt entry in config.';
    // }
    //
    // final model = GptModel.values.firstWhereOrNull((e) => e.id == gpt['model']);
    // if (model == null)
    // {
    //     throw 'Unknown model: ${gpt['model']}\nAvailable models: ${GptModel.values.map((e) => e.id).join(', ')}';
    // }
    //
    // final description = gpt['description'];
    // if (description == null)
    // {
    //     throw 'Missing description';
    // }
    //
    // return GptConfig(
    //     model: model,
    //     description: description,
    //     maxInputLength: gpt['max_input_length'] ?? model.defaultInputLength,
    //     temperature: gpt['temperature']?.toDouble(),
    //     excludes: (gpt['excludes'] as List?)
    //               ?.map((e) => I18nLocale.fromString(e))
    //               .toList() ??
    //               [],
    //     return new GptConfig();
    // }
}