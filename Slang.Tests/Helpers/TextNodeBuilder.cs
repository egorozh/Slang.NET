using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes;
using Slang.Generator.Core.Nodes.Nodes;

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
            extendData: null,
            new BuildModelConfig(
                KeyCase: CaseStyle.Pascal,
                KeyMapCase: CaseStyle.Camel,
                ParamCase: paramCase,
                PluralAuto: Slang.Generator.Core.Entities.PluralAuto.Off,
                PluralParameter: "n"),
            raw,
            modifiers: new Dictionary<string, string>()
        );
    }
}