using System.Text.RegularExpressions;

namespace Slang.Gpt.Utils;

public static partial class Regexes
{
    public static readonly Regex FileWithLocaleRegex = MyFileWithLocaleRegex();
    public static readonly Regex BaseFileRegex = MyBaseFileRegex();
    public static readonly Regex ModifierRegex = MyModifierRegex();
    public static readonly Regex UnicodeRegex = MyUnicodeRegex();

    [GeneratedRegex(FileWithLocaleRegular)]
    private static partial Regex MyFileWithLocaleRegex();

    [GeneratedRegex(BaseFileRegular)]
    private static partial Regex MyBaseFileRegex();

    [GeneratedRegex(ModifierRegular)]
    private static partial Regex MyModifierRegex();

    [GeneratedRegex(@"\\u(?<Value>[a-fA-F0-9]{4})")]
    private static partial Regex MyUnicodeRegex();
    
    /// Finds the parts of the locale. It must start with an underscore.
    /// groups for strings-zh-Hant-TW:
    /// 1 = strings
    /// 2 = zh (language, non-nullable)
    /// 3 = Hant (script)
    /// 4 = TW (country)
    private const string FileWithLocaleRegular =
        "^(?:([a-zA-Z0-9]+)[_-])?([a-z]{2,3})(?:[_-]([A-Za-z]{4}))?(?:[_-]([A-Z]{2}|[0-9]{3}))?$";

    /// matches any string without special characters
    private const string BaseFileRegular = "^([a-zA-Z0-9]+)?$";

    /// Matches the modifier part in a key if it exists
    /// greet(plural, param=gender)
    /// 1 - greet
    /// 2 - plural, param=gender
    private const string ModifierRegular = @"^(\w+)\((.+)\)$";
}