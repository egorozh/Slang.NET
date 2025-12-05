using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes.Utils;

namespace Slang.Tests.Unit.Utils;

public class ParseInterpolationTests
{
    [Test]
    public void CustomPlaceholderInterpolation()
    {
        var interpolation = NodeHelpers.ParseInterpolation(
            raw: """[{"Some":17,"Json":"1234"}]""",
            defaultType: "object",
            paramCase: CaseStyle.Camel,
            "{{",
            "}}"
        );

        Assert.IsEmpty(interpolation.Params);
        Assert.AreEqual("""[{"Some":17,"Json":"1234"}]""", interpolation.ParsedContent);
    }

    [Test]
    public void CustomPlaceholderInterpolationTest()
    {
        var interpolation = NodeHelpers.ParseInterpolation(
            raw: """some {{xxx: int}}""",
            defaultType: "object",
            paramCase: CaseStyle.Camel,
            "{{",
            "}}"
        );

        Assert.AreEqual(new Dictionary<string, string> { { "xxx", "int" } }, interpolation.Params);
        Assert.AreEqual("""some {xxx}""", interpolation.ParsedContent);
    }
}