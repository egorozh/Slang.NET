using System.Collections;
using System.Text.Json;
using Slang.Generator.Config.Entities;
using Slang.Generator.Nodes.Nodes;
using Slang.Generator.Nodes.Utils;
using Slang.Generator.Translations;
using NodeHelpers = Slang.Generator.Nodes.Utils.NodeHelpers;

namespace Slang.Generator.Nodes;

internal static partial class NodesRepository
{
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

            string? comment = !curr.ContainsKey($"@{key}")
                ? null
                : ParseCommentNode(curr[$"@{key}"]);

            var node = GetNode(
                currPath,
                comment,
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
        string? comment,
        BuildModelConfig config,
        Dictionary<string, ILeafNode> leavesMap,
        BuildModelResult? baseData,
        object? value,
        IReadOnlyDictionary<string, string> modifiers)
    {
        if (value is string or int or JsonElement {ValueKind: JsonValueKind.String})
        {
            var textNode = CreateTextNode(currPath, comment, config, value.ToString()!, modifiers);

            leavesMap[currPath] = textNode;

            return textNode;
        }

        if (value is IList or JsonElement {ValueKind: JsonValueKind.Array})
        {
            IList l;

            if (value is IList list)
            {
                l = list;
            }
            else
            {
                var jsonList = (JsonElement) value;
                l = jsonList.EnumerateArray().ToList();
            }

            // key: [ ...value ]
            // interpret the list as map
            Dictionary<string, object> listAsMap = l
                .Cast<object>()
                .Select((v, i) => (v, i))
                .ToDictionary(
                    v => v.i.ToString(),
                    v => v.v
                );


            var children = ParseMapNode(
                parentPath: currPath,
                curr: listAsMap,
                config: config,
                keyCase: config.KeyCase,
                leavesMap: leavesMap,
                baseData: baseData
            );

            // varly only take their values, ignoring keys
            var node = new ListNode(
                Path: currPath,
                Comment: comment,
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
            comment);
    }

    private static StringTextNode CreateTextNode(
        string path,
        string? comment,
        BuildModelConfig config,
        string value,
        IReadOnlyDictionary<string, string> modifiers)
    {
        (string? parsedContent, var paramTypeMap) = NodeHelpers.ParseInterpolation(
            raw: value,
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
            Comment: comment,
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
        string? comment)
    {
        Dictionary<string, object?> dict;

        switch (value)
        {
            case Dictionary<string, object?> dictionary:
                dict = dictionary;
                break;
            case JsonElement {ValueKind: JsonValueKind.Object} jsonElement:
                dict = [];

                foreach (var property in jsonElement.EnumerateObject())
                    dict.Add(property.Name, property.Value);

                break;
            default:
                return null;
        }

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
            {
                return null;
            }

            // split children by comma for plurals and contexts
            Dictionary<string, StringTextNode> digestedMap = [];

            var entries = children.ToList();

            foreach (var entry in entries)
            {
                string[] split = entry.Key.Split(KeyDelimiter);

                if (split.Length == 1)
                {
                    // keep as is
                    digestedMap[entry.Key] = entry.Value as StringTextNode;
                }
                else
                {
                    // split!
                    // {one,two: hi} -> {one: hi, two: hi}
                    foreach (var newChild in split)
                    {
                        digestedMap[newChild] = entry.Value as StringTextNode;
                    }
                }
            }

            string paramName = !modifiers.ContainsKey(NodeModifiers.Param)
                ? config.PluralParameter
                : modifiers[NodeModifiers.Param];

            string paramType = "int";

            foreach (var textNode in digestedMap.Values)
            {
                var tempType = textNode.ParamTypeMap[paramName];
                if (tempType != null &&
                    (textNode is StringTextNode && tempType != "object"))
                {
                    paramType = tempType;
                    break;
                }
            }

            varNode = new PluralNode(
                Path: currPath,
                Modifiers: modifiers,
                Comment: comment,
                PluralType: detectedType.NodeType == DetectionType.PluralCardinal
                    ? PluralType.Cardinal
                    : PluralType.Ordinal,
                Quantities: digestedMap.Select(v =>
                        new KeyValuePair<Quantity, StringTextNode>(v.Key.ToQuantity()!.Value, v.Value))
                    .ToDictionary(),
                ParamName: paramName,
                ParamType: paramType
            );
        }
        else
        {
            varNode = new ObjectNode(
                Path: currPath,
                Comment: comment,
                Modifiers: modifiers,
                Entries: children,
                IsMap: detectedType.NodeType == DetectionType.Map
            );
        }

        if (varNode is PluralNode node)
            leavesMap[currPath] = node;

        return varNode;
    }

    private static string? ParseCommentNode(object? node)
    {
        if (node == null)
            return null;

        if (node is string or JsonElement {ValueKind: JsonValueKind.String})
        {
            // parse string directly
            return node.ToString();
        }

        return null;
    }

    private static DetectionResult DetermineNodeType(
        BuildModelConfig config,
        IReadOnlyDictionary<string, string> modifiers,
        Dictionary<string, Node> children
    )
    {
        var modifierFlags = modifiers.Keys.ToHashSet();

        if (modifierFlags.Contains(NodeModifiers.Map))
            return new DetectionResult(DetectionType.Map);

        if (modifierFlags.Contains(NodeModifiers.Cardinal))
            return new DetectionResult(DetectionType.PluralCardinal);

        if (modifierFlags.Contains(NodeModifiers.Ordinal))
            return new DetectionResult(DetectionType.PluralOrdinal);

        var childrenSplitByComma =
            children.Keys.SelectMany(key => key.Split(KeyDelimiter)).ToList();

        if (childrenSplitByComma.Count == 0)
        {
            // fallback: empty node is a class by default
            return new DetectionResult(DetectionType.ClassType);
        }

        if (config.PluralAuto != PluralAuto.Off)
        {
            // check if every children is 'zero', 'one', 'two', 'few', 'many' or 'other'
            bool isPlural = childrenSplitByComma.Count <= Pluralization.AllQuantities.Length &&
                            childrenSplitByComma
                                .All(key =>
                                    Pluralization.AllQuantities.Any(q => q.ParamName().ToCase(config.KeyCase) == key));
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

    private enum DetectionType
    {
        ClassType,
        Map,
        PluralCardinal,
        PluralOrdinal
    }

    private record struct DetectionResult(DetectionType NodeType);
}