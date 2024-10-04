using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Tests;

public class MapsTests
{
    [Test]
    public void Should_remove_one_entry_with_ignoreMissing()
    {
        var map = new Dictionary<string, object?>
        {
            {"a(ignoreGpt)", "b"},
            {"c", "d"}
        };

        Maps.RemoveIgnoreGpt(map: map);

        Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
        {
            {"c", "d"}
        }));
    }

    [Test]
    public void Should_remove_nested_entry_with_ignoreMissing()
    {
        var map = new Dictionary<string, object?>
        {
            {
                "a", new Dictionary<string, object?>
                {
                    {"b(ignoreGpt)", "c"},
                    {"d", "e"}
                }
            }
        };

        Maps.RemoveIgnoreGpt(map: map);

        Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
        {
            {
                "a", new Dictionary<string, object?>
                {
                    {"d", "e"}
                }
            }
        }));
    }

    [Test]
    public void Should_not_remove_anything()
    {
        var map = new Dictionary<string, object?>
        {
            {"a", "b"},
            {"c", "d"}
        };

        Maps.RemoveIgnoreGpt(map: map);

        Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
        {
            {"a", "b"},
            {"c", "d"}
        }));
    }

    [Test]
    public void Should_extract_one_comment()
    {
        var map = new Dictionary<string, object?>
        {
            {"@a", "b"},
            {"c", "d"}
        };

        var comments = Maps.ExtractComments(map: map, remove: true);
        Assert.Multiple(() =>
        {
            Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
            {
                {"c", "d"},
            }));

            Assert.That(comments, Is.EqualTo(new Dictionary<string, object?>
            {
                {"@a", "b"},
            }));
        });
    }

    [Test]
    public void Should_extract_nested_comment()
    {
        var map = new Dictionary<string, object?>
        {
            {
                "a", new Dictionary<string, object?>
                {
                    {"@b", "c"},
                    {"d", "e"}
                }
            }
        };

        var comments = Maps.ExtractComments(map: map, remove: true);
        Assert.Multiple(() =>
        {
            Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
            {
                {
                    "a", new Dictionary<string, object?>
                    {
                        {"d", "e"}
                    }
                }
            }));

            Assert.That(comments, Is.EqualTo(new Dictionary<string, object?>
            {
                {
                    "a", new Dictionary<string, object?>
                    {
                        {"@b", "c"}
                    }
                }
            }));
        });
    }

    [Test]
    public void Should_remove_empty_map_after_extraction()
    {
        var map = new Dictionary<string, object?>
        {
            {
                "a", new Dictionary<string, object?>
                {
                    {"@b", "c"}
                }
            },
            {
                "f", new Dictionary<string, object?>
                {
                    {"g", "h"}
                }
            }
        };

        var comments = Maps.ExtractComments(map: map, remove: true);
        Assert.Multiple(() =>
        {
            Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
            {
                {
                    "f", new Dictionary<string, object?>
                    {
                        {"g", "h"}
                    }
                }
            }));

            Assert.That(comments, Is.EqualTo(new Dictionary<string, object?>
            {
                {
                    "a", new Dictionary<string, object?>
                    {
                        {"@b", "c"}
                    }
                }
            }));
        });
    }

    [Test]
    public void Should_not_remove()
    {
        var map = new Dictionary<string, object?>
        {
            {"@a", "b"},
            {"c", "d"}
        };

        var comments = Maps.ExtractComments(map: map, remove: false);
        Assert.Multiple(() =>
        {
            Assert.That(map, Is.EqualTo(new Dictionary<string, object?>
            {
                {"@a", "b"},
                {"c", "d"}
            }));

            Assert.That(comments, Is.EqualTo(new Dictionary<string, object?>
            {
                {"@a", "b"}
            }));
        });
    }
}