using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes.Nodes;
using Slang.Generator.Core.Nodes.Utils;
using Slang.Shared;

namespace Slang.Generator.Core.Nodes;

internal static partial class NodesRepository
{
    private static readonly IRawProvider RawProvider = new SystemTextJsonRawProvider();

    private const char KeyDelimiter = ','; // used by plural or context

    /// Takes the [curr] map which is (a part of) the raw tree from json / yaml
    /// and returns the node model.
    private static Dictionary<string, Node> ParseMapNode(
        string parentPath,
        Dictionary<string, object?> curr,
        BuildModelConfig config,
        CaseStyle? keyCase,
        Dictionary<string, ILeafNode> leavesMap,
        BuildModelResult? baseData)
    {
        Dictionary<string, Node> resultNodeTree = [];

        foreach ((string key, object? value) in curr)
        {
            if (key.StartsWith('@'))
            {
                // ignore comments
                continue;
            }

            (string path, var modifiers) = NodeUtils.ParseModifiers(key);

            string newKey = path.ToCase(keyCase);

            string currPath = !string.IsNullOrEmpty(parentPath) ? $"{parentPath}.{newKey}" : newKey;

            var extendData = !curr.ContainsKey($"@{key}")
                ? null
                : ParseExtendData(curr[$"@{key}"]);

            var node = GetNode(
                currPath,
                extendData,
                config,
                leavesMap,
                baseData,
                value,
                modifiers);

            if (node != null)
                resultNodeTree[newKey] = node;
        }

        return resultNodeTree;
    }

    private static Node? GetNode(
        string currPath,
        ExtendData? extendData,
        BuildModelConfig config,
        Dictionary<string, ILeafNode> leavesMap,
        BuildModelResult? baseData,
        object? value,
        IReadOnlyDictionary<string, string> modifiers)
    {
        if (RawProvider.TryGetString(value, out string raw))
        {
            var textNode = CreateTextNode(currPath, extendData, config, raw, modifiers);

            leavesMap[currPath] = textNode;

            return textNode;
        }

        if (RawProvider.TryGetArray(value, out var array))
        {
            // key: [ ...value ]
            // interpret the list as map
            var listAsMap = array
                .Select((v, i) => (v, i))
                .ToDictionary(
                    v => v.i.ToString(),
                    v => v.v
                );

            var children = ParseMapNode(
                parentPath: currPath,
                curr: listAsMap!,
                config: config,
                keyCase: config.KeyCase,
                leavesMap: leavesMap,
                baseData: baseData
            );

            // varly only take their values, ignoring keys
            var node = new ListNode(
                Path: currPath,
                ExtendData: extendData,
                Modifiers: modifiers,
                Entries: children.Values.ToList()
            );

            return node;
        }

        return CreateVarNode(
            config,
            leavesMap,
            baseData,
            value,
            currPath,
            modifiers,
            extendData);
    }

    internal static StringTextNode CreateTextNode(
        string path,
        ExtendData? extendData,
        BuildModelConfig config,
        string value,
        IReadOnlyDictionary<string, string> modifiers,
        bool shouldEscape = true)
    {
        (string? parsedContent, var paramTypeMap) = NodeHelpers.ParseInterpolation(
            raw: shouldEscape ? EscapeContent(value) : value,
            defaultType: "object",
            paramCase: config.ParamCase
        );

        var @params = paramTypeMap.Keys.ToHashSet();

        var parsedLinksResult = NodeHelpers.ParseLinks(
            input: parsedContent,
            linkParamMap: null
        );

        return new StringTextNode(
            Path: path,
            Modifiers: modifiers,
            ExtendData: extendData,
            Params: @params,
            ParamTypeMap: paramTypeMap,
            Links: parsedLinksResult.Links,
            Content: parsedLinksResult.ParsedContent,
            ParsedContent: parsedContent
        );
    }

    private static Node? CreateVarNode(
        BuildModelConfig config,
        Dictionary<string, ILeafNode> leavesMap,
        BuildModelResult? baseData,
        object? value,
        string currPath,
        IReadOnlyDictionary<string, string> modifiers,
        ExtendData? extendData)
    {
        var dict = RawProvider.GetDictionary(value);

        if (dict == null)
            return null;

        var children = ParseMapNode(
            parentPath: currPath,
            curr: dict,
            config: config,
            keyCase: config.KeyCase != config.KeyMapCase &&
                     modifiers.ContainsKey(NodeModifiers.Map)
                ? config.KeyMapCase
                : config.KeyCase,
            leavesMap: leavesMap,
            baseData: baseData
        );

        Node varNode;
        var detectedType = DetermineNodeType(config, modifiers, children);

        // split by comma if necessary
        if (detectedType.NodeType is DetectionType.PluralCardinal or DetectionType.PluralOrdinal)
        {
            if (children.Count == 0)
                return null;

            // split children by comma for plurals and contexts
            Dictionary<string, StringTextNode> digestedMap = [];

            foreach (var entry in children)
            {
                string[] split = entry.Key.Split(KeyDelimiter);

                var textNode = (StringTextNode)entry.Value;

                if (split.Length == 1)
                {
                    // keep as is
                    digestedMap[entry.Key] = textNode;
                }
                else
                {
                    // split!
                    // {one,two: hi} -> {one: hi, two: hi}
                    foreach (string newChild in split)
                    {
                        digestedMap[newChild] = textNode;
                    }
                }
            }

            string paramName = !modifiers.ContainsKey(NodeModifiers.Param)
                ? config.PluralParameter
                : modifiers[NodeModifiers.Param];

            string paramType = "int";

            foreach (var textNode in digestedMap.Values)
            {
                string? tempType = textNode.ParamTypeMap.ContainsKey(paramName)
                    ? textNode.ParamTypeMap[paramName]
                    : null;

                if (tempType != null && tempType != "object")
                {
                    paramType = tempType;
                    break;
                }
            }

            var pluralNode = new PluralNode(
                Path: currPath,
                Modifiers: modifiers,
                ExtendData: extendData,
                PluralType: detectedType.NodeType == DetectionType.PluralCardinal
                    ? PluralType.Cardinal
                    : PluralType.Ordinal,
                Quantities: digestedMap.ToDictionary(v => v.Key.ToQuantity()!.Value, v => v.Value),
                ParamName: paramName,
                ParamType: paramType
            );

            varNode = pluralNode;
            leavesMap[currPath] = pluralNode;
        }
        else
        {
            varNode = new ObjectNode(
                Path: currPath,
                ExtendData: extendData,
                Modifiers: modifiers,
                Entries: children,
                IsMap: detectedType.NodeType == DetectionType.Map
            );
        }

        return varNode;
    }

    private static ExtendData ParseExtendData(object? node)
    {
        if (RawProvider.TryGetString(node, out string comment))
            return new ExtendData(Description: comment);

        var dict = RawProvider.GetDictionary(node);

        if (dict != null)
            return ParseExtendData(dict);

        return new ExtendData(null);
    }

    private static ExtendData ParseExtendData(Dictionary<string, object?> map)
    {
        string? comment = null;
        Dictionary<string, Placeholder>? placeholders = null;

        if (map.ContainsKey("description") && RawProvider.TryGetString(map["description"], out string description))
        {
            comment = description;
        }

        if (map.ContainsKey("placeholders"))
        {
            var placeholderDict = RawProvider.GetDictionary(map["placeholders"]);

            if (placeholderDict?.Count > 0)
            {
                placeholders = new Dictionary<string, Placeholder>();

                foreach (var entry in placeholderDict)
                {
                    var value = RawProvider.GetDictionary(entry.Value);

                    if (value != null)
                        placeholders[entry.Key] = ParsePlaceholder(value);
                }
            }
        }

        return new ExtendData(comment, placeholders);
    }

    private static Placeholder ParsePlaceholder(Dictionary<string, object?> value)
    {
        string? type = null;
        string? format = null;

        if (value.ContainsKey("type") && RawProvider.TryGetString(value["type"], out string typeStr))
        {
            type = typeStr;
        }

        if (value.ContainsKey("format") && RawProvider.TryGetString(value["format"], out string formatStr))
        {
            format = formatStr;
        }

        return new Placeholder(type, format);
    }

    private static DetectionResult DetermineNodeType(
        BuildModelConfig config,
        IReadOnlyDictionary<string, string> modifiers,
        Dictionary<string, Node> children
    )
    {
        if (modifiers.ContainsKey(NodeModifiers.Map))
            return new DetectionResult(DetectionType.Map);

        if (modifiers.ContainsKey(NodeModifiers.Cardinal))
            return new DetectionResult(DetectionType.PluralCardinal);

        if (modifiers.ContainsKey(NodeModifiers.Ordinal))
            return new DetectionResult(DetectionType.PluralOrdinal);

        if (children.Keys.Count == 0)
        {
            // fallback: empty node is a class by default
            return new DetectionResult(DetectionType.ClassType);
        }

        if (config.PluralAuto != PluralAuto.Off)
        {
            // check if every children is 'zero', 'one', 'two', 'few', 'many' or 'other'
            bool isPlural = children.Keys.Count <= Pluralization.AllQuantities.Count &&
                            children.Keys
                                .SelectMany(key => key.Split(KeyDelimiter))
                                .All(key => Pluralization.AllQuantities.Contains(key.ToLower()));
            if (isPlural)
            {
                switch (config.PluralAuto)
                {
                    case PluralAuto.Cardinal:
                        return new DetectionResult(DetectionType.PluralCardinal);
                    case PluralAuto.Ordinal:
                        return new DetectionResult(DetectionType.PluralOrdinal);
                    case PluralAuto.Off:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // fallback: every node is a class by default
        return new DetectionResult(DetectionType.ClassType);
    }

    private static string EscapeContent(string raw)
    {
        return raw
            .Replace("\r\n", "\\n") // CRLF -> \n
            .Replace("\n", "\\n") // LF -> \n
            .Replace("\'", "\\\'"); // ' -> \'
    }

    private enum DetectionType
    {
        ClassType,
        Map,
        PluralCardinal,
        PluralOrdinal
    }

    private record struct DetectionResult(DetectionType NodeType);
}