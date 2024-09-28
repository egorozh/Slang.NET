using Microsoft.CodeAnalysis;
using Slang.Generator.Data;
using Slang.Generator.Domain;
using Slang.Generator.Domain.Entities;

namespace Slang.Generator.SourceGenerator.CodeBuilder;

internal static class TranslationsCodeBuilder
{
    public static async Task Generate(
        SourceProductionContext context,
        RawConfig config,
        SlangFileCollection fileCollection)
    {
        // STEP 2: scan translations
        var translationMap = await TranslationsRepository.Build(config, fileCollection: fileCollection);

        // STEP 3: generate .g.dart content
        var result = GeneratorFacade.Generate(
            rawConfig: config,
            translationComposition: translationMap
        );

        context.AddSource($"{config.ClassName}.g.cs", result.Header);

        foreach ((var locale, string localeTranslations) in result.Translations)
        {
            context.AddSource($"{config.ClassName}_{locale.TwoLetterISOLanguageName}.g.cs", localeTranslations);
        }
    }
}