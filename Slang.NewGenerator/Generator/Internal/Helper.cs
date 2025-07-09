using System.Globalization;
using System.Text;
using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes.Utils;

namespace Slang.Generator.Core.Generator.Internal;

public static class Helper
{
    private const char TabChar = '\t';

    public static void AppendWithTab(this StringBuilder builder, string text, int tabCount)
    {
        for (int i = 0; i < tabCount; i++)
            builder.Append(TabChar);

        builder.Append(text);
    }

    public static void AppendLineWithTab(this StringBuilder builder, string text, int tabCount)
    {
        for (int i = 0; i < tabCount; i++)
            builder.Append(TabChar);

        builder.AppendLine(text);
    }

    public static void AppendWithTab(this StringBuilder builder, char text, int tabCount)
    {
        for (int i = 0; i < tabCount; i++)
            builder.Append(TabChar);

        builder.Append(text);
    }

    /// Returns the class name of the root translation class.
    public static string GetClassNameRoot(
        string baseName,
        CultureInfo? locale
    )
    {
        string result = baseName.ToCase(CaseStyle.Pascal) +
                        (locale != null
                            ? locale.ToString().ToCaseOfLocale(CaseStyle.Pascal)
                            : string.Empty);

        return result;
    }

    public static string GetClassName(
        string parentName,
        string childName = "",
        CultureInfo? locale = null
    )
    {
        string languageTag = locale != null ? locale.ToString().ToCaseOfLocale(CaseStyle.Pascal) : string.Empty;

        return $"{parentName}{childName.ToCase(CaseStyle.Pascal)}{languageTag}";
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
}