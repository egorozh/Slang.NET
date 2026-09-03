using System.Globalization;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;

namespace Slang.Tests.Integration.Main;

/// <summary>
/// Files that differ only by their script subtag must stay separate locales.
/// See https://github.com/egorozh/Slang.NET/issues/5
/// </summary>
public class ScriptLocaleTests
{
    private const string Cyrillic = """{ "common": { "save": "ЋИРИЛИЦА-RS" } }""";
    private const string Latin = """{ "common": { "save": "LATINICA-RS" } }""";

    [Test]
    public async Task Files_differing_only_by_script_are_separate_locales()
    {
        var baseCulture = new CultureInfo("sr-Cyrl-RS");

        var fileCollection = FilesRepository.GetFileCollection(baseCulture, allFiles:
        [
            ("strings_sr-Cyrl-RS.i18n.json", Cyrillic),
            ("strings_sr-Latn-RS.i18n.json", Latin)
        ]);

        var composition = await TranslationsRepository.Build(baseCulture, fileCollection);

        Assert.That(composition.Keys, Is.EquivalentTo(new[]
        {
            new CultureInfo("sr-Cyrl-RS"),
            new CultureInfo("sr-Latn-RS")
        }));
    }

    [Test]
    public async Task Both_scripts_are_generated()
    {
        var baseCulture = new CultureInfo("sr-Cyrl-RS");

        var fileCollection = FilesRepository.GetFileCollection(baseCulture, allFiles:
        [
            ("strings_sr-Cyrl-RS.i18n.json", Cyrillic),
            ("strings_sr-Latn-RS.i18n.json", Latin)
        ]);

        var composition = await TranslationsRepository.Build(baseCulture, fileCollection);

        var result = GeneratorFacade.Generate(
            rawConfig: ConfigRepository.Create(
                inputFileName: "strings",
                @namespace: "Slang.Tests",
                className: "TestLocales",
                baseLocale: "sr-Cyrl-RS"
            ),
            composition,
            new DateTime(2024, 1, 1, 12, 0, 0)
        );

        Assert.Multiple(() =>
        {
            Assert.That(result.Header, Does.Contain("_sr_Cyrl_RS = new CultureInfo(\"sr-Cyrl-RS\")"));
            Assert.That(result.Header, Does.Contain("_sr_Latn_RS = new CultureInfo(\"sr-Latn-RS\")"));
            Assert.That(result.Translations[new CultureInfo("sr-Cyrl-RS")], Does.Contain("ЋИРИЛИЦА-RS"));
            Assert.That(result.Translations[new CultureInfo("sr-Latn-RS")], Does.Contain("LATINICA-RS"));
        });
    }
}
