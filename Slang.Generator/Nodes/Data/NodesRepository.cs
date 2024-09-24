using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Nodes.Domain;
using Slang.Generator.Nodes.Nodes;
using Slang.Generator.Translations.Domain;

namespace Slang.Generator.Nodes.Data;

internal static partial class NodesRepository
{
    public static BuildModelConfig ToBuildModelConfig(RawConfig rawConfig) => new(
        FallbackStrategy: rawConfig.FallbackStrategy,
        KeyCase: CaseStyle.Pascal,
        KeyMapCase: CaseStyle.Camel,
        ParamCase: CaseStyle.Camel,
        PluralAuto: rawConfig.PluralAuto,
        PluralParameter: rawConfig.PluralParameter
    );


    /// Builds the i18n model for ONE locale
    ///
    /// The [map] must be of type Map<String, dynamic> and all children may of type
    /// String, num, List<dynamic> or Map<String, dynamic>.
    ///
    /// If [baseData] is set and [BuildModelConfig.fallbackStrategy] is [FallbackStrategy.baseLocale],
    /// then the base translations will be added to contexts where the translation is missing.
    ///
    /// [handleLinks] can be set false to ignore links and leave them as is
    /// e.g. ${_root.greet(name: name} will be ${_root.greet}
    /// This is used for "Translation Overrides" where the links are resolved
    /// on invocation.
    ///
    /// [shouldEscapeText] can be set false to ignore escaping of text nodes
    /// e.g. "Let"s go" will be "Let"s go" instead of "Let\"s go".
    /// Similar to [handleLinks], this is used for "Translation Overrides".
    public static BuildModelResult GetNodes(
        BuildModelConfig buildConfig,
        TranslationsMap map,
        BuildModelResult? baseData = null,
        bool handleLinks = true
    )
    {
        // flat map for leaves (TextNode, PluralNode, ContextNode)
        Dictionary<string, ILeafNode> leavesMap = [];

        // 1st iteration: Build nodes according to given map
        //
        // Linked Translations:
        // They will be tracked but not handled
        // Assumption: They are basic linked translations without parameters
        // Reason: Not all TextNodes are built, so var parameters are unknown
        var resultNodeTree = ParseMapNode(
            parentPath: "",
            parentRawPath: "",
            curr: map,
            config: buildConfig,
            keyCase: buildConfig.KeyCase,
            leavesMap: leavesMap,
            baseData: baseData
        );

        // 2nd iteration: Handle parameterized linked translations
        //
        // TextNodes with parameterized linked translations are rebuilt with correct parameters.
        if (handleLinks)
            ResolveLinks(buildConfig, leavesMap);

        // imaginary root node
        var root = new ObjectNode(
            Path: "",
            Comment: null,
            Modifiers: new CustomDictionary<string, string>([]),
            Entries: resultNodeTree,
            IsMap: false);

        return new BuildModelResult(Root: root);
    }

    private static void ResolveLinks(BuildModelConfig buildConfig, Dictionary<string, ILeafNode> leavesMap)
    {
        var textLeavesNodes = leavesMap
            .Where(entry => entry.Value is StringTextNode)
            .Select(entry => entry.Value)
            .Cast<StringTextNode>();

        foreach (var value in textLeavesNodes)
        {
            Dictionary<string, HashSet<string>> linkParamMap = [];
            Dictionary<string, string> paramTypeMap = [];

            foreach (string link in value.Links)
            {
                HashSet<string> paramSet = [];

                List<string> visitedLinks = [];

                Queue<string> pathQueue = new();

                pathQueue.Enqueue(link);

                while (pathQueue.Count > 0)
                {
                    string currLink = pathQueue.Dequeue();
                    var linkedNode = leavesMap[currLink];

                    if (linkedNode == null)
                        throw new Exception(
                            "\"$key\" in <$localeDebug> is linked to \"$currLink\" but \"$currLink\" is undefined.");

                    visitedLinks.Add(currLink);

                    if (linkedNode is StringTextNode textNode)
                    {
                        paramSet.AddRange(textNode.Params);
                        paramTypeMap.AddAll(textNode.ParamTypeMap);

                        // lookup links
                        foreach (string child in textNode.Links)
                        {
                            if (!visitedLinks.Contains(child))
                                pathQueue.Enqueue(child);
                        }
                    }
                    else if (linkedNode is PluralNode pluralNode)
                    {
                        IEnumerable<StringTextNode> textNodes = pluralNode.Quantities.Values;
                        ;

                        foreach (var textNode2 in textNodes)
                        {
                            paramSet.AddRange(textNode2.Params);
                            paramTypeMap.AddAll(textNode2.ParamTypeMap);
                        }

                        if (linkedNode is PluralNode pluralNode2)
                        {
                            paramSet.Add(pluralNode2.ParamName);
                            paramTypeMap[pluralNode2.ParamName] = pluralNode2.ParamType;
                        }

                        // lookup links of children
                        foreach (var element in textNodes)
                        foreach (string child in element.Links)
                        {
                            if (!visitedLinks.Contains(child))
                                pathQueue.Enqueue(child);
                        }
                    }
                    else
                    {
                        throw new Exception(
                            "\"$key\" is linked to \"$currLink\" which is a ${linkedNode.runtimeType} (must be $TextNode or $ObjectNode).");
                    }
                }

                linkParamMap[link] = paramSet;
            }

            if (linkParamMap.Values.Any(@params => @params.Count > 0))
            {
                // rebuild TextNode because its linked translations have parameters
                UpdateWithLinkParams(
                    (StringTextNode) value,
                    linkParamMap: linkParamMap,
                    paramTypeMap: paramTypeMap
                );
            }
        }
    }

    private static void UpdateWithLinkParams(
        StringTextNode value,
        Dictionary<string, HashSet<string>> linkParamMap,
        Dictionary<string, string> paramTypeMap)
    {
        value.ParamTypeMap = paramTypeMap;
        value.Params.AddRange(linkParamMap.Values.SelectMany(e => e));

        // build a temporary TextNode to get the updated content
        var temp = CreateTextNode(
            value: value,
            linkParamMap: linkParamMap
        );

        value.Content = temp.Content;
    }

    private static StringTextNode CreateTextNode(
        StringTextNode value,
        Dictionary<string, HashSet<string>> linkParamMap)
    {
        var @params = value.Params;

        @params.AddRange(linkParamMap.Values.SelectMany(e => e));

        var parsedLinksResult = NodeHelpers.ParseLinks(
            input: value.ParsedContent,
            linkParamMap: linkParamMap
        );

        return new StringTextNode(
            Path: value.Path,
            Modifiers: value.Modifiers,
            Comment: value.Comment,
            Params: @params,
            ParamTypeMap: value.ParamTypeMap,
            Links: parsedLinksResult.Links,
            Content: parsedLinksResult.ParsedContent,
            ParsedContent: value.ParsedContent
        );
    }
}