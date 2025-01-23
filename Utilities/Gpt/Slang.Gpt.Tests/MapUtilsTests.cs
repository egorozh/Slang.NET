using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Tests;

public class MapUtilsTests
{
    [Test]
    public void Should_subtract_a_single_value()
    {
        var result = MapUtils.Subtract(
            target: new Dictionary<string, object?> {{"a", 42}},
            other: new Dictionary<string, object?> {{"a", 33}}
        );

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>()));
    }

    [Test]
    public void Should_ignore_missing_values()
    {
        var result = MapUtils.Subtract(
            target: new Dictionary<string, object?> {{"a", 42}},
            other: new Dictionary<string, object?> {{"b", 33}}
        );

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"a", 42}
        }));
    }

    [Test]
    public void Should_keep_whole_map()
    {
        var result = MapUtils.Subtract(
            target: new Dictionary<string, object?>
            {
                {"a", 42},
                {
                    "b", new Dictionary<string, object?>
                    {
                        {"a", true}
                    }
                }
            },
            other: new Dictionary<string, object?> {{"a", 42}}
        );

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {
                "b", new Dictionary<string, object?>
                {
                    {"a", true}
                }
            }
        }));
    }

    [Test]
    public void Should_subtract_map_partially()
    {
        var result = MapUtils.Subtract(
            target: new Dictionary<string, object?>
            {
                {"a", 42},
                {
                    "b", new Dictionary<string, object?>
                    {
                        {"c", true},
                        {"d", false}
                    }
                }
            },
            other: new Dictionary<string, object?>
            {
                {"a", 42},
                {
                    "b", new Dictionary<string, object?>
                    {
                        {"c", true}
                    }
                }
            }
        );

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {
                "b", new Dictionary<string, object?>
                {
                    {"d", false}
                }
            }
        }));
    }
}