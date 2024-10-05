using System.Globalization;
using System.Reflection;
using Slang.Generator.Data;
using Slang.Generator.Domain;

namespace Slang.Tests.Integration.Main;

public class JsonTests
{
    private string _enInput;
    private string _deInput;
    private string _expectedOutputHeader;
    private string _expectedOutputEn;
    private string _expectedOutputDe;

    [SetUp]
    public void Setup()
    {
        _enInput = LoadResource("Slang.Tests.Integration.Resources.json_en.json");
        _deInput = LoadResource("Slang.Tests.Integration.Resources.json_de.json");
        _expectedOutputHeader = LoadResource("Slang.Tests.Integration.Resources._expected_header.output");
        _expectedOutputEn = LoadResource("Slang.Tests.Integration.Resources._expected_en.output");
        _expectedOutputDe = LoadResource("Slang.Tests.Integration.Resources._expected_de.output");
    }

    [Test]
    public void Json()
    {
        CultureInfo en = new("en");
        CultureInfo de = new("de");

        var result = GeneratorFacade.Generate(
            rawConfig: ConfigRepository.Create(
                inputFileName: "json",
                @namespace: "Slang.Tests",
                className: "TestLocales"
            ),
            new TranslationComposition
            {
                {en, TranslationsDecoder.DecodeWithFileType(_enInput)},
                {de, TranslationsDecoder.DecodeWithFileType(_deInput)}
            },
            new DateTime(2024, 1, 1, 12, 0, 0)
        );
        Assert.Multiple(() =>
        {
            Assert.That(result.Header, Is.EqualTo(_expectedOutputHeader));
            Assert.That(result.Translations[en], Is.EqualTo(_expectedOutputEn));
            Assert.That(result.Translations[de], Is.EqualTo(_expectedOutputDe));
        });
    }

    private static string LoadResource(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(path);

        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }
}