using System.Globalization;
using Slang.Gpt.Domain.Models;
using Slang.Gpt.Domain.Prompt;

namespace Slang.Gpt.Tests;

public class PromptTests
{
    private const string ExpectedSystemPrompt =
        """
        The user wants to internationalize the app. The user will provide you with a JSON file containing the English strings.
        You will translate it to German.
        Parameters are interpolated with {parameter}.
        Linked translations are denoted with the \"@:path0.path1\" syntax.

        Here is the app description. Respect this context when translating:
        A simple calculator
        """;
    
    [Test]
    public void Should_return_a_prompt()
    {
        var prompts = Prompt.GetPrompts(
            targetCulture: new CultureInfo("de"),
            config: new GptConfig(
                BaseCulture: new CultureInfo("en"),
                Model: GptModel.Gpt35Turbo,
                Description: "A simple calculator",
                MaxInputLength: 1000,
                Temperature: null,
                Excludes: []
            ),
            translations: new Dictionary<string, object?>
            {
                ["calculate"] = "Calculate",
                ["add"] = "Add",
                ["subtract"] = "Subtract",
                ["multiply"] = "Multiply",
                ["divide"] = "Divide"
            }
        );

        Assert.That(prompts, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(prompts.First().System, Is.EqualTo(ExpectedSystemPrompt));
            Assert.That(
                prompts.First().User, Is.EqualTo(
                    """
                    {
                      "calculate": "Calculate",
                      "add": "Add",
                      "subtract": "Subtract",
                      "multiply": "Multiply",
                      "divide": "Divide"
                    }
                    """
                ));
        });
    }

    [Test]
    public void Should_divide_into_smaller_prompts()
    {
        var prompts = Prompt.GetPrompts(
            targetCulture: new CultureInfo("de"),
            config: new GptConfig(
                BaseCulture: new CultureInfo("en"),
                Model: GptModel.Gpt35Turbo,
                Description: "A simple calculator",
                MaxInputLength: 1,
                Temperature: null,
                Excludes: []
            ),
            translations: new Dictionary<string, object?>
            {
                ["a"] = "a",
                ["b"] = "b",
                ["c"] = "c",
                ["d"] = "d",
                ["e"] = "e"
            }
        );

        Assert.That(prompts, Has.Count.EqualTo(5));
        Assert.Multiple(() =>
        {
            Assert.That(prompts.First().System, Is.EqualTo(ExpectedSystemPrompt));
            Assert.That(prompts.First().User, Is.EqualTo(
                """
                {
                  "a": "a"
                }
                """
            ));
        });
        Assert.Multiple(() =>
        {
            Assert.That(prompts[1].System, Is.EqualTo(ExpectedSystemPrompt));
            Assert.That(prompts[1].User, Is.EqualTo(
                """
                {
                  "b": "b"
                }
                """
            ));
        });
        Assert.Multiple(() =>
        {
            Assert.That(prompts[2].System, Is.EqualTo(ExpectedSystemPrompt));
            Assert.That(prompts[2].User, Is.EqualTo(
                """
                {
                  "c": "c"
                }
                """
            ));
        });

        Assert.Multiple(() =>
        {
            Assert.That(prompts[3].System, Is.EqualTo(ExpectedSystemPrompt));
            Assert.That(prompts[3].User, Is.EqualTo(
                """
                {
                  "d": "d"
                }
                """
            ));
        });

        Assert.Multiple(() =>
        {
            Assert.That(prompts[4].System, Is.EqualTo(ExpectedSystemPrompt));
            Assert.That(prompts[4].User, Is.EqualTo(
                """
                {
                  "e": "e"
                }
                """
            ));
        });
    }
}