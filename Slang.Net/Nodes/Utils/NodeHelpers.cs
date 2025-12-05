using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes.Nodes;
using Slang.Shared;

namespace Slang.Generator.Core.Nodes.Utils;

internal static class NodeHelpers
{
    public static string DetermineGenericType(IReadOnlyCollection<Node> entries)
    {
        const string dynamicType = "dynamic";

        if (entries.All(child => child is StringTextNode { ParamTypeMap.Count: 0 }))
            return "string";

        if (entries.All(child => child is ListNode))
        {
            string childGenericType = ((ListNode)entries.First()).GenericType;

            foreach (var node in entries)
            {
                var child = (ListNode)node;

                if (childGenericType != child.GenericType)
                {
                    childGenericType = dynamicType; // default
                }
            }

            return $"List<{childGenericType}>"; // all lists have the same generic type
        }

        if (entries.All(child => child is ObjectNode { IsMap: true }))
        {
            string childGenericType = ((ObjectNode)entries.First()).GenericType;

            foreach (var node in entries)
            {
                var child = (ObjectNode)node;

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

        string parsedContent = Regexes.LinkedRegex.Replace(input,
            match =>
            {
                string linkedPath = (!string.IsNullOrEmpty(match.Groups[1].Value)
                    ? match.Groups[1].Value
                    : match.Groups[2].Value).ToCaseWithDots(CaseStyle.Pascal);

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
        CaseStyle? paramCase,
        string startCharacter,
        string endCharacter
    )
    {
        Dictionary<string, string> @params = [];

        string parsedContent = ReplaceBracesInterpolation(
            raw,
            startCharacter: startCharacter,
            endCharacter: endCharacter,
            replacer: match =>
            {
                string rawParam = match.Substring(startCharacter.Length, match.Length - (startCharacter.Length + endCharacter.Length));
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
        if (rawParam.EndsWith(")"))
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
        string s,
        Func<string, string> replacer,
        string startCharacter,
        string endCharacter
    )
    {
        return ReplaceBetween(
            input: s,
            startCharacter: startCharacter,
            endCharacter: endCharacter,
            replacer: replacer
        );
    }

    private static string ReplaceBetween(
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
            int startIndex = curr.IndexOf(startCharacter, StringComparison.Ordinal);

            if (startIndex == -1)
            {
                buffer.Append(curr);
                break;
            }

            if (startIndex >= 1 && curr[startIndex - 1] == '\\')
            {
                // ignore because of preceding \
                int length = startIndex - 1;
                if (length > 0) // на случай, если startIndex == 1
                    buffer.Append(curr, 0, length); // перегрузка StringBuilder.Append(string, int, int)
                buffer.Append(startCharacter);
                if (startIndex + 1 < curr.Length)
                {
                    int offset = startIndex + startCharacterLength;
                    curr = offset < curr.Length
                        ? curr.Substring(offset) // начиная с offset до конца строки
                        : string.Empty; // защитимся от выхода за пределы
                    continue;
                }

                break;
            }

            if (startIndex >= 2 &&
                curr[startIndex - 1] == ':' &&
                curr[startIndex - 2] == '@')
            {
                // ignore because of preceding @: which indicates an escaped, linked translation
                buffer.Append(curr.Substring(0, startIndex + 1));

                if (startIndex + 1 < curr.Length)
                {
                    curr = curr.Substring(startIndex + startCharacterLength);
                    continue;
                }

                break;
            }

            if (startIndex != 0)
            {
                // add prefix
                if (startIndex > 0) // чтобы не бросить ArgumentOutOfRangeException
                    buffer.Append(curr, 0, startIndex);
            }

            int endIndex = curr.IndexOf(endCharacter, startIndex + startCharacterLength, StringComparison.Ordinal);
            if (endIndex == -1)
            {
                int count = curr.Length - startIndex;
                if (count > 0) // защищаемся от пустого хвоста
                    buffer.Append(curr, startIndex, count);
                break;
            }

            int length2 = (endIndex + endCharacterLength) - startIndex;
            if (length2 > 0) // защита от отрицательной / нулевой длины
            {
                string slice = curr.Substring(startIndex, length2);
                buffer.Append(replacer(slice));
            }
            int offset2 = endIndex + endCharacterLength;

            curr = offset2 < curr.Length
                ? curr.Substring(offset2) // вся строка с offset до конца
                : string.Empty;
        } while (!string.IsNullOrEmpty(curr));

        return buffer.ToString();
    }
}