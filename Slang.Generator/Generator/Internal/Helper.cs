using System.Globalization;
using System.Text;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Utils;

namespace Slang.Generator.Generator;

public static class Helper
{
    private const char nullFlag = '\0';

    /// Pragmatic way to detect links within interpolations.
    private const string characteristicLinkPrefix = "_root.";

    /// Returns the class name of the root translation class.
    public static string GetClassNameRoot(
        string baseName,
        CultureInfo? locale
    )
    {
        string result = baseName.ToCase(CaseStyle.Pascal) +
                        (locale != null
                            ? locale.TwoLetterISOLanguageName.ToCaseOfLocale(CaseStyle.Pascal)
                            : string.Empty);

        return result;
    }

    public static string GetClassName(
        string parentName,
        string childName = "",
        CultureInfo? locale = null
    )
    {
        string languageTag = locale != null ? locale.TwoLetterISOLanguageName.ToCaseOfLocale(CaseStyle.Pascal) : "";

        return parentName + childName.ToCase(CaseStyle.Pascal) + languageTag;
    }


    /// Either returns the plain string or the obfuscated one.
    /// Whenever translation strings gets rendered, this method must be called.
    public static string GetStringLiteral(string value, int linkCount)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // Return the plain version
            if (value.StartsWith("${") &&
                value.IndexOf('}') == value.Length - 1 &&
                linkCount == 1)
            {
                // We can just remove the ${ and } since it's already a string
                return value.Substring(2, value.Length - 1);
            }

            // We need to add quotes
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }
        
        return string.Empty;
    }

    /// Replaces every ${x} with the result of [replacer].
    /// Escaped \${x} will be transformed to ${x} without replacer call
    public static string ReplaceDartNormalizedInterpolation(this string s, Func<string, string> replacer)
    {
        return _replaceBetween(
            input: s,
            startCharacter: "${",
            endCharacter: "}",
            replacer: replacer
        );
    }


    private static string _replaceBetween(
        string input,
        string startCharacter,
        string endCharacter,
        Func<string, string> replacer)
    {
        string curr = input;
        StringBuilder buffer = new();

        int startCharacterLength = startCharacter.Length;
        int endCharacterLength = endCharacter.Length;

        do
        {
            int startIndex = curr.IndexOf(startCharacter);

            if (startIndex == -1)
            {
                buffer.Append(curr);
                break;
            }

            if (startIndex >= 1 && curr[startIndex - 1] == '\\')
            {
                // ignore because of preceding \
                buffer.Append(curr[..(startIndex - 1)]); // do not include \
                buffer.Append(startCharacter);
                if (startIndex + 1 < curr.Length)
                {
                    curr = curr[(startIndex + startCharacterLength)..];
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (startIndex != 0)
            {
                // add prefix
                buffer.Append(curr[..startIndex]);
            }

            int endIndex = curr.IndexOf(endCharacter, startIndex + startCharacterLength);

            if (endIndex == -1)
            {
                buffer.Append(curr[startIndex..]);
                break;
            }

            buffer.Append(replacer(curr.Substring(startIndex, endIndex + endCharacterLength)));
            curr = curr[(endIndex + endCharacterLength)..];
        } while (curr.Length > 0);

        return buffer.ToString();
    }
}