using Slang.Generator.Domain.Entities;
using Slang.Generator.Domain.Nodes;
using Slang.Generator.Domain.Nodes.Nodes;

namespace Slang.Tests.Helpers;

internal static class TextNodeBuilder
{
    public static StringTextNode TextNode(
        string raw,
        CaseStyle? paramCase = null
    )
    {
        return NodesRepository.CreateTextNode(
            path: "",
            comment: null,
            new BuildModelConfig(
                KeyCase: CaseStyle.Pascal,
                KeyMapCase: CaseStyle.Camel,
                ParamCase: paramCase,
                PluralAuto: PluralAuto.Off,
                PluralParameter: "n"),
            raw,
            modifiers: new Dictionary<string, string>()
        );
    }
}