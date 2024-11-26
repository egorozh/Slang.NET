using Microsoft.CodeAnalysis;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;

namespace Slang.Generator.CodeBuilder;

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
            translationComposition: translationMap,
            DateTime.Now
        );

        context.AddSource($"{config.ClassName}.g.cs", result.Header);

        foreach ((var locale, string localeTranslations) in result.Translations)
        {
            context.AddSource($"{config.ClassName}_{locale.TwoLetterISOLanguageName}.g.cs", localeTranslations);
        }
    }
}