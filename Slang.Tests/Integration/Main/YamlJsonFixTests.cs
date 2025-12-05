using System.Globalization;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;

namespace Slang.Tests.Integration.Main;

public class YamlJsonTests
{
    private string _enInput;
    private string _expectedOutputEn;

    [SetUp]
    public void Setup()
    {
        _enInput = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources.yaml_en_json.yaml");
        _expectedOutputEn = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources._expected_en_json.output");
    }

    [Test]
    public void Yaml()
    {
        CultureInfo en = new("en");

        var result = GeneratorFacade.Generate(
            rawConfig: ConfigRepository.Create(
                inputFileName: "yaml",
                @namespace: "Slang.Tests",
                className: "TestLocales",
                startCharacter: "{{",
                endCharacter: "}}"
            ),
            new TranslationComposition
            {
                { en, TranslationsDecoder.DecodeWithFileType(_enInput, "yaml") }
            },
            new DateTime(2024, 1, 1, 12, 0, 0)
        );

        Console.Write(result.Translations[en]);

        Assert.That(result.Translations[en], Is.EqualTo(_expectedOutputEn));
    }
}