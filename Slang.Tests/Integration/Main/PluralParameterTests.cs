using System.Globalization;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;
using Slang.Tests.Helpers;

namespace Slang.Tests.Integration.Main;

/// <summary>
/// Placeholders shared by several plural forms must be collected once.
/// See https://github.com/egorozh/Slang.NET/issues/6
/// </summary>
public class PluralParameterTests
{
    private const string Ru = """
                              {
                                "line": {
                                  "items": {
                                    "one":   "{time: string} · {n} позиция",
                                    "few":   "{time: string} · {n} позиции",
                                    "other": "{time: string} · {n} позиций"
                                  }
                                }
                              }
                              """;

    private const string PartiallyTyped = """
                                          {
                                            "line": {
                                              "items": {
                                                "one":   "{time} · {n} позиция",
                                                "few":   "{time: string} · {n} позиции",
                                                "other": "{time} · {n} позиций"
                                              }
                                            }
                                          }
                                          """;

    private static (string Header, string Translation) Generate(string json)
    {
        var ru = new CultureInfo("ru-RU");

        var result = GeneratorFacade.Generate(
            rawConfig: ConfigRepository.Create(
                inputFileName: "strings",
                @namespace: "Slang.Tests",
                className: "TestLocales",
                baseLocale: "ru-RU"
            ),
            new TranslationComposition
            {
                {ru, TranslationsDecoder.DecodeWithFileType(json)}
            },
            new DateTime(2024, 1, 1, 12, 0, 0)
        );

        return (result.Header, result.Translations[ru]);
    }

    [Test]
    public void Shared_placeholder_is_emitted_once()
    {
        (string header, string translation) = Generate(Ru);

        Assert.That(translation, Does.Contain("public virtual string Items(int n, string time) =>"));

        CompilationAssert.Compiles(header, translation);
    }

    [Test]
    public void Declared_type_wins_over_untyped_forms()
    {
        (string header, string translation) = Generate(PartiallyTyped);

        Assert.That(translation, Does.Contain("public virtual string Items(int n, string time) =>"));

        CompilationAssert.Compiles(header, translation);
    }
}
