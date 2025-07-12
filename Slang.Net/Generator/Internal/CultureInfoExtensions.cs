using System.Globalization;

namespace Slang.Generator.Core.Generator.Internal;

internal static class CultureInfoExtensions
{
    public static string ToSafeName(this CultureInfo culture)
    {
        return culture.ToString().Replace("-", "_");
    }
}