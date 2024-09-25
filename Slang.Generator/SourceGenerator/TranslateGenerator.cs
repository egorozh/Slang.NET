using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slang.Generator.SourceGenerator.CodeBuilder;
using Slang.Generator.SourceGenerator.Extensions;
using Slang.Generator.SourceGenerator.Models;

namespace Slang.Generator.SourceGenerator;

public record TranslationsParam(
    string InputFileName,
    string InputFilePattern,
    string BaseLocale,
    string PluralAuto,
    string? InputDirectory,
    string PluralParameter
);

[Generator]
public class TranslateGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<Result<(HierarchyInfo Hierarchy, TranslationsParam? Info)>> generationInfoWithErrors =
            context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "Slang.TranslationsAttribute",
                    static (node, _) => node is ClassDeclarationSyntax classDeclaration &&
                                        classDeclaration.HasOrPotentiallyHasAttributes(),
                    (context, token) =>
                    {
                        INamedTypeSymbol typeSymbol = (INamedTypeSymbol) context.TargetSymbol;

                        // Gather all generation info, and any diagnostics
                        TranslationsParam? info = ValidateTargetTypeAndGetInfo(context.Attributes[0]);

                        token.ThrowIfCancellationRequested();

                        // If there are any diagnostics, there's no need to compute the hierarchy info at all, just return them

                        HierarchyInfo hierarchy = HierarchyInfo.From(typeSymbol);

                        token.ThrowIfCancellationRequested();

                        return new Result<(HierarchyInfo, TranslationsParam?)>((hierarchy, info));
                    })
                .Where(static item => item is not null);

        // Get the filtered sequence to enable caching
        IncrementalValuesProvider<(HierarchyInfo Hierarchy, TranslationsParam Info)> generationInfo =
            generationInfoWithErrors.Select(static (item, _) => item.Value)!;

        var configProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (p, ctx) => p.GlobalOptions)
            .Select(static (g, ctx) =>
            {
                if (!g.TryGetValue("build_property.projectdir", out string? projectDir))
                    return null;

                return projectDir;
            });

        context.RegisterSourceOutput(generationInfo.Combine(configProvider), static (productionContext, data) =>
        {
            (var (hierarchy, info), string? projectDir) = data;

            if (string.IsNullOrEmpty(projectDir))
                return;

            TranslationsCodeBuilder.Generate(productionContext, hierarchy, info, projectDir);
        });
    }


    private static TranslationsParam ValidateTargetTypeAndGetInfo(AttributeData attributeData)
    {
        string? InputFileName = attributeData.GetNamedArgument("InputFileName", "");
        string? InputFilePattern = attributeData.GetNamedArgument("InputFilePattern", "");
        string? BaseLocale = attributeData.GetNamedArgument("BaseLocale", "");
        string? PluralAuto = attributeData.GetNamedArgument("PluralAuto", "");
        string? InputDirectory = attributeData.GetNamedArgument("InputDirectory", "");
        string? PluralParameter = attributeData.GetNamedArgument("PluralParameter", "");

        return new TranslationsParam(
            InputFileName: InputFileName,
            InputFilePattern: InputFilePattern,
            InputDirectory: InputDirectory,
            BaseLocale: BaseLocale,
            PluralAuto: PluralAuto,
            PluralParameter: PluralParameter
        );
    }
}