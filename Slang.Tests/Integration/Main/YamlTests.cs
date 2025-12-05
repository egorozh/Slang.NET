using System.Globalization;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;

namespace Slang.Tests.Integration.Main;

public class YamlTests
{
    private string _enInput;
    private string _deInput;
    private string _expectedOutputHeader;
    private string _expectedOutputEn;
    private string _expectedOutputDe;

    [SetUp]
    public void Setup()
    {
        _enInput = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources.yaml_en.yaml");
        _deInput = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources.yaml_de.yaml");
        _expectedOutputHeader = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources._expected_header.output");
        _expectedOutputEn = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources._expected_en.output");
        _expectedOutputDe = EmbeddedLoader.LoadResource("Slang.Tests.Integration.Resources._expected_de.output");
    }

    [Test]
    public void Yaml()
    {
        CultureInfo en = new("en");
        CultureInfo de = new("de");

        var result = GeneratorFacade.Generate(
            rawConfig: ConfigRepository.Create(
                inputFileName: "yaml",
                @namespace: "Slang.Tests",
                className: "TestLocales"
            ),
            new TranslationComposition
            {
                { en, TranslationsDecoder.DecodeWithFileType(_enInput, "yaml") },
                { de, TranslationsDecoder.DecodeWithFileType(_deInput, "yaml") }
            },
            new DateTime(2024, 1, 1, 12, 0, 0)
        );
        
        Console.Write(result.Translations[en]);
        Assert.Multiple(() =>
        {
            Assert.That(result.Header, Is.EqualTo(_expectedOutputHeader));
            Assert.That(result.Translations[en], Is.EqualTo(_expectedOutputEn));
            Assert.That(result.Translations[de], Is.EqualTo(_expectedOutputDe));
        });
    }
}