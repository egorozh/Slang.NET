namespace Slang.Generator.Domain.Nodes.Utils;

public record struct NodePathInfo(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers
);

public static class NodeUtils
{
    public static readonly Dictionary<string, string> Empty = new();


    /// Returns a map containing modifiers
    /// greet(param=gender, rich)
    /// will result in
    /// {param: gender, rich: rich}
    public static NodePathInfo ParseModifiers(string originalKey)
    {
        var match = Regexes.ModifierRegex.Match(originalKey);

        if (!match.Success)
            return new NodePathInfo(Path: originalKey, Modifiers: Empty);

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
            Modifiers: resultMap
        );
    }
}