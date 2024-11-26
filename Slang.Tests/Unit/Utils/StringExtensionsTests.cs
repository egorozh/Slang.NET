using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes.Utils;

namespace Slang.Tests.Unit.Utils;

public class StringExtensionsTests
{
    [Test]
    public void Empty()
    {
        Assert.That("".Capitalize(), Is.EqualTo(""));
    }

    [Test]
    public void OneCharacter()
    {
        Assert.That("e".Capitalize(), Is.EqualTo("E"));
    }

    [Test]
    public void MoreCharacters()
    {
        Assert.That("heLLo".Capitalize(), Is.EqualTo("Hello"));
    }

    [Test]
    public void NoTransformation()
    {
        Assert.That("hello_worldCool".ToCase(null), Is.EqualTo("hello_worldCool"));
    }

    [Test]
    public void CamelToCamel()
    {
        Assert.That("helloWorldCool".ToCase(CaseStyle.Camel), Is.EqualTo("helloWorldCool"));
    }

    [Test]
    public void snake_to_camel()
    {
        Assert.That("hello_world_cool".ToCase(CaseStyle.Camel), Is.EqualTo("helloWorldCool"));
    }

    [Test]
    public void pascal_to_camel()
    {
        Assert.That("HelloWorldCool".ToCase(CaseStyle.Camel), Is.EqualTo("helloWorldCool"));
    }

    [Test]
    public void camel_to_pascal()
    {
        Assert.That("helloWorldCool".ToCase(CaseStyle.Pascal), Is.EqualTo("HelloWorldCool"));
    }

    [Test]
    public void snake_to_pascal()
    {
        Assert.That("hello_world_cool".ToCase(CaseStyle.Pascal), Is.EqualTo("HelloWorldCool"));
    }
}