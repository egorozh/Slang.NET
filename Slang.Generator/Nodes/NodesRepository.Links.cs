using Slang.Generator.Nodes.Nodes;
using Slang.Generator.Nodes.Utils;

namespace Slang.Generator.Nodes;

internal static partial class NodesRepository
{
    private static void ResolveLinks(Dictionary<string, ILeafNode> leavesMap)
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
                    value,
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