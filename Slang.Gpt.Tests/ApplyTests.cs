using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Tests;

public class ApplyTests
{
    [Test]
    public void Ignore_strings_in_baseMap_if_not_applied()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: new Dictionary<string, object?> {{"base", "BB"}},
            newMap: new Dictionary<string, object?> {{"new", "NN"}},
            oldMap: new Dictionary<string, object?> {{"old", "OO"}},
            verbose: false
        );

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"new", "NN"},
            {"old", "OO"}
        }));

        Assert.That(result.Keys.ToList(), Is.EqualTo(new List<string>
        {
            "old", "new"
        }));
    }

    [Test]
    public void handle_empty_newMap()
    {
        var map = new Dictionary<string, object?>
        {
            {
                "a", new Dictionary<string, object?>
                {
                    {"aa", "AA"}
                }
            }
        };

        var result = MapUtils.ApplyMapRecursive(
            baseMap: map,
            newMap: [],
            oldMap: map,
            verbose: false
        );

        Assert.That(result, Is.EqualTo(map));
    }

    [Test]
    public void handle_empty_newMap_another_variant()
    {
        var map = new Dictionary<string, object?>
        {
            {
                "a", new Dictionary<string, object?>
                {
                    {"aa", "AA"}
                }
            }
        };

        var result = MapUtils.ApplyMapRecursive(
            baseMap: [],
            newMap: [],
            oldMap: map,
            verbose: false);

        Assert.That(result, Is.EqualTo(map));
    }

    [Test]
    public void apply_new_strings()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: [],
            newMap: new Dictionary<string, object?>
            {
                {"d4", "D"}
            },
            oldMap: new Dictionary<string, object?>
            {
                {"c1", "C"},
                {"a2", "A"},
                {"b3", "B"}
            },
            verbose: false);

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"c1", "C"},
            {"a2", "A"},
            {"b3", "B"},
            {"d4", "D"}
        }));

        Assert.That(result.Keys.ToList(), Is.EqualTo(new List<string>
        {
            "c1", "a2", "b3", "d4"
        }));
    }

    [Test]
    public void add_string_to_populated_map_but_respect_order_from_base()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: new Dictionary<string, object?>
            {
                {"c1", "cc"},
                {"d4", "dd"},
                {"a2", "aa"},
                {"b3", "bb"}
            },
            newMap: new Dictionary<string, object?>
            {
                {"d4", "D"}
            },
            oldMap: new Dictionary<string, object?>
            {
                {"c1", "C"},
                {"a2", "A"},
                {"b3", "B"}
            },
            verbose: false);

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"c1", "C"},
            {"d4", "D"},
            {"a2", "A"},
            {"b3", "B"}
        }));

        Assert.That(result.Keys.ToList(), Is.EqualTo(new List<string>
        {
            "c1", "d4", "a2", "b3"
        }));
    }

    [Test]
    public void also_reorder_even_if_newMap_is_empty()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: new Dictionary<string, object?>
            {
                {"c", "cc"},
                {"a", "aa"},
                {"b", "bb"}
            },
            newMap: [],
            oldMap: new Dictionary<string, object?>
            {
                {"b", "B"},
                {"c", "C"},
                {"d", "D"},
                {"a", "A"}
            },
            verbose: false);

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"c", "C"},
            {"a", "A"},
            {"b", "B"},
            {"d", "D"}
        }));

        Assert.That(result.Keys.ToList(), Is.EqualTo(new List<string>
        {
            "c", "a", "b", "d"
        }));
    }

    [Test]
    public void also_reorder_the_nested_map()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: new Dictionary<string, object?>
            {
                {"c", "cc"},
                {
                    "a", new Dictionary<string, object?>
                    {
                        {"w", "ww"},
                        {"y", "yy"},
                        {"x", "xx"},
                        {"z", "zz"},
                    }
                },
                {
                    "b", "bb"
                }
            },
            newMap: new Dictionary<string, object?>
            {
                {
                    "a", new Dictionary<string, object?>
                    {
                        {"x", "X"}
                    }
                }
            },
            oldMap: new Dictionary<string, object?>
            {
                {"c", "C"},
                {
                    "a", new Dictionary<string, object?>
                    {
                        {"0", "0"},
                        {"z", "Z"},
                        {"y", "Y"},
                    }
                },
                {
                    "b", "B"
                }
            },
            verbose: false);

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"c", "C"},
            {
                "a", new Dictionary<string, object?>
                {
                    {"y", "Y"},
                    {"x", "X"},
                    {"z", "Z"},
                    {"0", "0"}
                }
            },
            {"b", "B"}
        }));

        Assert.That(result.Keys.ToList(), Is.EqualTo(new List<string>
        {
            "c", "a", "b"
        }));
        Assert.That((result["a"] as Dictionary<string, object?>)!.Keys.ToList(), Is.EqualTo(new List<string>
        {
            "y", "x", "z", "0"
        }));
    }

    [Test]
    public void ignore_new_modifier()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: [],
            newMap: new Dictionary<string, object?>
            {
                {"a(param=arg0)", "A"}
            },
            oldMap: [],
            verbose: false);

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"a", "A"}
        }));
    }

    [Test]
    public void apply_modifier_of_base_map()
    {
        var result = MapUtils.ApplyMapRecursive(
            baseMap: new Dictionary<string, object?>
            {
                {"a(param=arg0)", "base"}
            },
            newMap: new Dictionary<string, object?>
            {
                {"a", "new"}
            },
            oldMap: new Dictionary<string, object?>
            {
                {"a", "old"}
            },
            verbose: false);

        Assert.That(result, Is.EqualTo(new Dictionary<string, object?>
        {
            {"a(param=arg0)", "new"}
        }));
    }
}