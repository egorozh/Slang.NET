using System.Text.RegularExpressions;
using Slang.Generator.Utils;

namespace Slang.Generator.Nodes.Data;

public record NodePathInfo(
    string Path,
    CustomDictionary<string, string> Modifiers
);

public static partial class NodeUtils
{
    private static readonly Regex ModifierRegex = MyModifierRegex();

    /// Returns a map containing modifiers
    /// greet(param=gender, rich)
    /// will result in
    /// {param: gender, rich: rich}
    public static NodePathInfo ParseModifiers(string originalKey)
    {
        var match = ModifierRegex.Match(originalKey);

        if (!match.Success)
            return new NodePathInfo(Path: originalKey, Modifiers: new CustomDictionary<string, string>([]));

        string[] modifiers = match.Groups[2].Value.Split(",");

        Dictionary<string, string> resultMap = [];

        foreach (string modifier in modifiers)
        {
            if (modifier.Contains('='))
            {
                string[] parts = modifier.Split("=");

                if (parts.Length != 2)
                    throw new Exception("Hints must be in format \"key:value\" or \"key\"");

                resultMap[parts[0].Trim()] = parts[1].Trim();
            }
            else
            {
                string modifierDigested = modifier.Trim();
                resultMap[modifierDigested] = modifierDigested;
            }
        }

        return new NodePathInfo(
            Path: match.Groups[1].Value,
            Modifiers: new CustomDictionary<string, string>(resultMap)
        );
    }

    /// Matches the modifier part in a key if it exists
    /// greet(plural, param=gender)
    /// 1 - greet
    /// 2 - plural, param=gender
    [GeneratedRegex(@"^(\w+)\((.+)\)$")]
    private static partial Regex MyModifierRegex();
}