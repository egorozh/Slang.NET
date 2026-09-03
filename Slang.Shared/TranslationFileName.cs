using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Slang.Shared;

/// <summary>
/// The locale information encoded in a translation file name.
/// </summary>
/// <param name="Namespace">The part before the locale ("strings" for "strings_zh-Hant-TW").</param>
/// <param name="LocaleTag">
/// The BCP-47 tag built from the file name ("zh-Hant-TW"), or null for a base file
/// that carries no locale ("strings").
/// </param>
public readonly record struct TranslationFileName(string Namespace, string? LocaleTag);

public static class TranslationFileNames
{
    /// <summary>
    /// Parses a translation file name (with or without extension) into its namespace and locale tag.
    /// Returns null when the name is neither a base file nor a file with a locale.
    /// </summary>
    public static TranslationFileName? Parse(string fileName)
    {
        string name = Path.GetFileNameWithoutExtension(fileName).Split('.')[0];

        // base file (file without locale, may be multiples due to namespaces!)
        // could also be a non-base locale when directory name is a locale
        if (Regexes.BaseFileRegex.IsMatch(name))
            return new TranslationFileName(name, null);

        // secondary files (strings_x)
        var match = Regexes.FileWithLocaleRegex.Match(name);

        if (!match.Success)
            return null;

        string language = match.Groups[2].Value.ToLowerInvariant();
        string script = NormalizeScript(match.Groups[3].Value);
        string country = match.Groups[4].Value.ToUpperInvariant();

        string tag = language;

        if (script.Length > 0)
            tag += "-" + script;

        if (country.Length > 0)
            tag += "-" + country;

        return new TranslationFileName(match.Groups[1].Value, tag);
    }

    /// <summary>
    /// Creates the culture for a tag produced by <see cref="Parse"/>.
    /// </summary>
    /// <remarks>
    /// Not every well-formed tag is a culture the host knows: .NET Framework — the compiler host
    /// inside Visual Studio — rejects unknown names outright. Such a tag is retried without its
    /// script subtag so generation keeps working; two files that then collapse into one locale are
    /// reported as a duplicate instead of being dropped silently.
    /// </remarks>
    public static bool TryCreateCulture(string localeTag, out CultureInfo culture)
    {
        foreach (string candidate in GetCandidates(localeTag))
        {
            try
            {
                culture = new CultureInfo(candidate);
                return true;
            }
            catch (CultureNotFoundException)
            {
                // try the next, less specific candidate
            }
        }

        culture = CultureInfo.InvariantCulture;
        return false;
    }

    private static IEnumerable<string> GetCandidates(string localeTag)
    {
        yield return localeTag;

        string[] parts = localeTag.Split('-');

        bool hasScript = parts.Length > 1 && parts[1].Length == 4;

        if (hasScript)
            yield return parts.Length == 3 ? parts[0] + "-" + parts[2] : parts[0];

        if (parts.Length > 1)
            yield return parts[0];
    }

    private static string NormalizeScript(string script)
    {
        if (script.Length == 0)
            return script;

        return char.ToUpperInvariant(script[0]) + script.Substring(1).ToLowerInvariant();
    }
}
