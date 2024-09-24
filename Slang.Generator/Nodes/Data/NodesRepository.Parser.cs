using System.Collections;
using System.Text.Json;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Nodes.Domain;
using Slang.Generator.Nodes.Nodes;
using Slang.Generator.Translations.Domain;
using Slang.Generator.Utils;

namespace Slang.Generator.Nodes.Data;

internal static partial class NodesRepository
{
    private const char KeyDelimiter = ','; // used by plural or context

    /// Takes the [curr] map which is (a part of) the raw tree from json / yaml
    /// and returns the node model.
    private static Dictionary<string, Node> ParseMapNode(
        string parentPath,
        string parentRawPath,
        TranslationsMap curr,
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

            string currRawPath = !string.IsNullOrEmpty(parentRawPath) ? $"{parentRawPath}.{key}" : key;

            string newKey = path.ToCase(keyCase);

            string currPath = !string.IsNullOrEmpty(parentPath) ? $"{parentPath}.{newKey}" : newKey;

            string? comment = _parseCommentNode(curr[$"@{key}"]);

            var node = GetNode(
                currPath,
                currRawPath,
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
        string currRawPath,
        string? comment,
        BuildModelConfig config,
        Dictionary<string, ILeafNode> leavesMap,
        BuildModelResult? baseData,
        object? value,
        CustomDictionary<string, string> modifiers)
    {
        if (value is string or int or JsonElement {ValueKind: JsonValueKind.String})
        {
            if (config.FallbackStrategy == FallbackStrategy.baseLocaleEmptyString && value is string s &&
                string.IsNullOrEmpty(s))
            {
                return null;
            }

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
                l = JsonSerializer.Deserialize<IList>(jsonList.GetRawText())!;
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
                parentRawPath: currRawPath,
                curr: new TranslationsMap(listAsMap),
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
            currRawPath,
            modifiers,
            comment);
    }

    private static StringTextNode CreateTextNode(
        string path,
        string? comment,
        BuildModelConfig config,
        string value,
        CustomDictionary<string, string> modifiers)
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
        string currRawPath,
        CustomDictionary<string, string> modifiers,
        string? comment)
    {
        Dictionary<string, object> dict;

        switch (value)
        {
            case Dictionary<string, object> dictionary:
                dict = dictionary;
                break;
            case JsonElement {ValueKind: JsonValueKind.Object} jsonElement:
                dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonElement.GetRawText())!;
                break;
            default:
                return null;
        }

        var children = ParseMapNode(
            parentPath: currPath,
            parentRawPath: currRawPath,
            curr: new TranslationsMap(dict),
            config: config,
            keyCase: config.KeyCase != config.KeyMapCase &&
                     modifiers.ContainsKey(NodeModifiers.Map)
                ? config.KeyMapCase
                : config.KeyCase,
            leavesMap: leavesMap,
            baseData: baseData
        );
        Node varNode;
        var detectedType = _determineNodeType(config, currPath, modifiers, children);

        // split by comma if necessary
        if (detectedType.NodeType is DetectionType.PluralCardinal or DetectionType.PluralOrdinal)
        {
            if (children.Count == 0)
            {
                switch (config.FallbackStrategy)
                {
                    case FallbackStrategy.none:
                        throw new Exception(
                            "\"$currPath\" in <$localeDebug> is empty but it is marked for pluralization / context. Define \"fallback_strategy: base_locale\" to ignore this node.");
                    case FallbackStrategy.baseLocale:
                    case FallbackStrategy.baseLocaleEmptyString:
                        return null;
                }
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


            string paramName = modifiers[NodeModifiers.Param] ?? config.PluralParameter;
            string paramType = "int";

            foreach (var textNode in digestedMap.Values)
            {
                var tempType = textNode.ParamTypeMap[paramName];
                if (tempType != null &&
                    ((textNode is StringTextNode && tempType != "object")))
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
                Quantities: digestedMap.Select((v) =>
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

    private static string? _parseCommentNode(object? node)
    {
        if (node == null)
            return null;

        if (node is string s)
        {
            // parse string directly
            return s;
        }

        if (node is Dictionary<string, object> dictionary)
        {
            // ARB style
            return dictionary["description"].ToString();
        }

        return null;
    }

    private static DetectionResult _determineNodeType(
        BuildModelConfig config,
        string nodePath,
        CustomDictionary<string, string> modifiers,
        Dictionary<string, Node> children
    )
    {
        var modifierFlags = modifiers.Keys.ToHashSet();

        if (modifierFlags.Contains(NodeModifiers.Map))
        {
            return new DetectionResult(DetectionType.Map);
        }

        if (modifierFlags.Contains(NodeModifiers.Cardinal))
        {
            return new DetectionResult(DetectionType.PluralCardinal);
        }

        if (modifierFlags.Contains(NodeModifiers.Ordinal))
        {
            return new DetectionResult(DetectionType.PluralOrdinal);
        }

        var childrenSplitByComma =
            children.Keys.SelectMany(key => key.Split(KeyDelimiter)).ToList();

        if (childrenSplitByComma.Count == 0)
        {
            // fallback: empty node is a class by default
            return new DetectionResult(DetectionType.ClassType);
        }

        if (config.PluralAuto != PluralAuto.off)
        {
            // check if every children is 'zero', 'one', 'two', 'few', 'many' or 'other'
            bool isPlural = childrenSplitByComma.Count <= Pluralization.AllQuantities.Length &&
                            childrenSplitByComma
                                .All((key) =>
                                    Pluralization.AllQuantities.Any(q => q.ParamName().ToCase(config.KeyCase) == key));
            if (isPlural)
            {
                switch (config.PluralAuto)
                {
                    case PluralAuto.cardinal:
                        return new DetectionResult(DetectionType.PluralCardinal);
                    case PluralAuto.ordinal:
                        return new DetectionResult(DetectionType.PluralOrdinal);
                    case PluralAuto.off:
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

    private record DetectionResult(
        DetectionType NodeType
    );
}