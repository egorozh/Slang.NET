using System.Text;
using System.Text.RegularExpressions;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Nodes.Data;
using Slang.Generator.Nodes.Nodes;

namespace Slang.Generator.Nodes.Domain;

internal static partial class NodeHelpers
{
    private static readonly Regex LinkedRegex = MyLinkedRegex();

    public static string DetermineGenericType(IReadOnlyCollection<Node> entries)
    {
        const string dynamicType = "dynamic";

        if (entries.All(child => child is StringTextNode {Params.Count: 0}))
            return "string";

        if (entries.All(child => child is ListNode))
        {
            string? childGenericType = ((ListNode)entries.First()).GenericType;

            foreach (ListNode child in entries)
            {
                if (childGenericType != child.GenericType)
                {
                    childGenericType = dynamicType; // default
                }
            }

            return $"List<{childGenericType}>"; // all lists have the same generic type
        }

        if (entries.All(child => child is ObjectNode {IsMap: true}))
        {
            string? childGenericType = ((ObjectNode) entries.First()).GenericType;

            foreach (ObjectNode child in entries)
            {
                if (childGenericType != child.GenericType)
                {
                    childGenericType = dynamicType; // default
                }
            }

            return $"Dictionary<string, {childGenericType}>"; // all maps have same generics
        }

        return dynamicType;
    }

    internal record struct ParseLinksResult(string ParsedContent, HashSet<string> Links);

    public static ParseLinksResult ParseLinks(string input, Dictionary<string, HashSet<string>>? linkParamMap)
    {
        HashSet<string> links = [];

        string parsedContent = LinkedRegex.Replace(input,
            match =>
            {
                string linkedPath = match.Groups[1].Value.ToCaseWithDots(CaseStyle.Pascal);

                links.Add(linkedPath);

                if (linkParamMap == null)
                {
                    // assume no parameters
                    return $"{{_root.{linkedPath}}}";
                }

                var linkedParams = linkParamMap[linkedPath];

                if (linkedParams.Count == 0)
                {
                    return $"{{_root.{linkedPath}}}";
                }

                string parameterString = string.Join(", ", linkedParams.Select(param => $"{param}: {param}"));

                return $"{{_root.{linkedPath}({parameterString})}}";
            });

        return new ParseLinksResult(parsedContent, links);
    }

    /// <param name="ParsedContent"></param>
    /// <param name="Params"> Map of parameter name -> parameter type</param>
    internal record struct ParseInterpolationResult(string ParsedContent, Dictionary<string, string> Params);

    public static ParseInterpolationResult ParseInterpolation(
        string raw,
        string defaultType,
        CaseStyle? paramCase
    )
    {
        Dictionary<string, string> @params = [];

        string parsedContent = ReplaceBracesInterpolation(raw, replacer: match =>
        {
            string rawParam = match[1.. ^1];
            var parsedParam = ParseParam(rawParam: rawParam, defaultType: defaultType, caseStyle: paramCase);
            @params[parsedParam.ParamName] = parsedParam.ParamType;
            return $"{{{parsedParam.ParamName}}}";
        });

        return new ParseInterpolationResult(parsedContent, @params);
    }

    private record struct ParseParamResult(string ParamName, string ParamType);

    private static ParseParamResult ParseParam(
        string rawParam,
        string defaultType,
        CaseStyle? caseStyle
    )
    {
        if (rawParam.EndsWith(')'))
        {
            // rich text parameter with default value
            // this will be parsed by parseParamWithArg
            return new ParseParamResult(rawParam, string.Empty);
        }

        string[] split = rawParam.Split(':');

        return split.Length == 1
            ? new ParseParamResult(split[0].ToCase(caseStyle), defaultType)
            : new ParseParamResult(split[0].Trim().ToCase(caseStyle), split[1].Trim());
    }

    /// Replaces every {x} with the result of [replacer].
    private static string ReplaceBracesInterpolation(
        string s, Func<string, string> replacer
    )
    {
        return ReplaceBetween(
            input: s,
            startCharacter: "{",
            endCharacter: "}",
            replacer: replacer
        );
    }

    private static string ReplaceBetween(string input,
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
            int startIndex = curr.IndexOf(startCharacter, StringComparison.Ordinal);

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

                break;
            }

            if (startIndex != 0)
            {
                // add prefix
                buffer.Append(curr[0..startIndex]);
            }

            int endIndex = curr.IndexOf(endCharacter, startIndex + startCharacterLength, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                buffer.Append(curr[startIndex..]);
                break;
            }

            buffer.Append(replacer(curr[startIndex..(endIndex + endCharacterLength)]));
            curr = curr[(endIndex + endCharacterLength)..];
        } while (!string.IsNullOrEmpty(curr));

        return buffer.ToString();
    }

    /// matches @:translation.key
    [GeneratedRegex(@"@:(\w[\w|.]*\w|\w)")]
    private static partial Regex MyLinkedRegex();
}