using System.Globalization;

namespace Slang.Gpt.Domain.Utils;

public static class Locales
{
    private static readonly Dictionary<string, string[]> Presets = new()
    {
        {"gdp-3", ["zh", "es", "ja"]},
        {"gdp-5", ["zh", "es", "ja", "de", "fr"]},
        {"gdp-10", ["zh", "es", "ja", "de", "fr", "pt", "ar", "it", "ru", "ko"]},
        {"eu-3", ["de", "fr", "it"]},
        {"eu-5", ["de", "fr", "it", "es", "pl"]},
        {"eu-10", ["de", "fr", "it", "es", "pl", "ro", "nl", "cs", "el", "sv"]},
    };

    /// <summary>
    /// Returns the preset locales for the given [id].
    /// </summary>
    public static List<CultureInfo>? GetPreset(string id)
    {
        if (!Presets.TryGetValue(id, out string[]? locales))
            return null;

        return locales.Select(l => new CultureInfo(l)).ToList();
    }
}