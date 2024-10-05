using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Tests;

public class NodeUtilsTests
{
    [Test]
    public void Returns_the_original_string_when_no_modifiers_are_present()
    {
        const string input = "greet";
        Assert.That(input.WithoutModifiers(), Is.EqualTo("greet"));
    }

    [Test]
    public void Removes_modifiers_from_a_string_with_key_value_modifier()
    {
        const string input = "greet(param=gender)";
        Assert.That(input.WithoutModifiers(), Is.EqualTo("greet"));
    }

    [Test]
    public void Removes_modifiers_from_a_string_with_key_only_modifier()
    {
        const string input = "greet(rich)";
        Assert.That(input.WithoutModifiers(), Is.EqualTo("greet"));
    }

    [Test]
    public void Removes_modifiers_from_a_string_with_multiple_modifiers()
    {
        const string input = "greet(param=gender, rich)";
        Assert.That(input.WithoutModifiers(), Is.EqualTo("greet"));
    }
}