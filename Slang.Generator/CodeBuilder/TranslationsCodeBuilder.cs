using Microsoft.CodeAnalysis;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;
using Slang.Generator.Diagnostics;

namespace Slang.Generator.CodeBuilder;

internal static class TranslationsCodeBuilder
{
    public static async Task Generate(
        SourceProductionContext context,
        RawConfig config,
        SlangFileCollection fileCollection)
    {
        ReportDuplicateLocales(context, fileCollection);

        // STEP 2: scan translations
        var translationMap = await TranslationsRepository.Build(config.BaseLocale, fileCollection: fileCollection);

        // STEP 3: generate .g.dart content
        var result = GeneratorFacade.Generate(
            rawConfig: config,
            translationComposition: translationMap,
            DateTime.Now
        );

        context.AddSource($"{config.ClassName}.g.cs", result.Header);

        foreach ((var locale, string localeTranslations) in result.Translations)
        {
            context.AddSource($"{config.ClassName}_{locale}.g.cs", localeTranslations);
        }
    }

    /// <summary>
    /// Only the first file of a locale is generated, so files sharing a locale would silently
    /// lose their translations - warn instead.
    /// </summary>
    private static void ReportDuplicateLocales(SourceProductionContext context, SlangFileCollection fileCollection)
    {
        var duplicates = fileCollection.Files
            .GroupBy(file => file.Locale)
            .Where(group => group.Count() > 1);

        foreach (var group in duplicates)
        {
            string[] fileNames = group.Select(file => $"'{file.FileName}'").ToArray();

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.DuplicateLocale,
                location: null,
                string.Join(", ", fileNames),
                group.Key,
                group.First().FileName));
        }
    }
}
