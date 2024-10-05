using Slang.Shared;

namespace Slang.Tests.Unit.Utils;

public class NodeUtilsTests
{
    [Test]
    public void No_modifiers_returns_an_empty_map()
    {
        var result = NodeUtils.ParseModifiers("greet");

        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo("greet"));
            Assert.That(result.Modifiers, Is.EqualTo(new Dictionary<string, string>()));
        });
    }

    [Test]
    public void Single_key_value_modifier_returns_a_map_with_one_entry()
    {
        var result = NodeUtils.ParseModifiers("greet(param=gender)");

        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo("greet"));
            Assert.That(result.Modifiers, Is.EqualTo(new Dictionary<string, string>
            {
                {"param", "gender"}
            }));
        });
    }

    [Test]
    public void Single_key_only_modifier_returns_a_map_with_one_entry()
    {
        var result = NodeUtils.ParseModifiers("greet(rich)");
        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo("greet"));
            Assert.That(result.Modifiers, Is.EqualTo(new Dictionary<string, string>
            {
                {"rich", "rich"}
            }));
        });
    }

    [Test]
    public void Multiple_modifiers_return_a_map_with_multiple_entries()
    {
        var result = NodeUtils.ParseModifiers("greet(param=gender, rich)");
        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo("greet"));
            Assert.That(result.Modifiers, Is.EqualTo(new Dictionary<string, string>
            {
                {"param", "gender"},
                {"rich", "rich"}
            }));
        });
    }

    [Test]
    public void Extra_spaces_are_trimmed()
    {
        var result = NodeUtils.ParseModifiers("greet( param = gender , rich )");
        Assert.Multiple(() =>
        {
            Assert.That(result.Path, Is.EqualTo("greet"));
            Assert.That(result.Modifiers, Is.EqualTo(new Dictionary<string, string>
            {
                {"param", "gender"},
                {"rich", "rich"}
            }));
        });
    }
}