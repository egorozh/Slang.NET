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
            // targets:
            //     $default:
            // builders:
            // slang_build_runner:
            // options:
            // base_locale: en
            // input_file_pattern: .i18n.json # will be ignored anyways because we put in manually
            // output_file_name: translations.cgm.dart # currently set manually for each test
            // output_format: single_file # may get changed programmatically
            // locale_handling: true # may get changed programmatically
            // string_interpolation: braces
            // timestamp: false # make every test deterministic
            // maps:
            // - end.pages.0
            //     - end.pages.1
            // interfaces:
            // PageData: onboarding.pages.*
            //     EndData: end
            rawConfig: ConfigRepository.Create(
                inputFileName: "json",
                @namespace: "Slang.Tests",
                className: "TestLocales"
            ),
            new TranslationComposition
            {
                {en, TranslationsDecoder.DecodeWithFileType(_enInput)},
                {de, TranslationsDecoder.DecodeWithFileType(_deInput)},
            }
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
        
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}