using System.Text.RegularExpressions;

namespace Slang.Shared;

public static class Regexes
{
    public static readonly Regex FileWithLocaleRegex = new(FileWithLocaleRegular, RegexOptions.Compiled);
    public static readonly Regex BaseFileRegex = new(BaseFileRegular, RegexOptions.Compiled);
    public static readonly Regex LinkedRegex = new(LinkedRegular, RegexOptions.Compiled);
    public static readonly Regex ModifierRegex = new(ModifierRegular, RegexOptions.Compiled);
    public static readonly Regex UnicodeRegex = new(UnicodeRegular, RegexOptions.Compiled);

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

    /// matches @:translation.key or @:{translation.key}, but not \@:translation.key
    /// 1 = argument of @:translation.key
    /// 2 = argument of @:{translation.key}
    private const string LinkedRegular = @"(?<!\\)@:(?:(\w[\w|.]*\w|\w)|\{(\w[\w|.]*\w|\w)\})";

    /// Matches the modifier part in a key if it exists
    /// greet(plural, param=gender)
    /// 1 - greet
    /// 2 - plural, param=gender
    private const string ModifierRegular = @"^(\w+)\((.+)\)$";

    private const string UnicodeRegular = @"\\u(?<Value>[a-fA-F0-9]{4})";
}
