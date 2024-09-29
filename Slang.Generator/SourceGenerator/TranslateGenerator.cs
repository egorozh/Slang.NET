using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slang.Generator.Data;
using Slang.Generator.SourceGenerator.CodeBuilder;
using Slang.Generator.SourceGenerator.Extensions;
using Slang.Generator.SourceGenerator.Models;

namespace Slang.Generator.SourceGenerator;

public record TranslationsParam(
    string? InputFileName,
    string? BaseLocale,
    string? PluralAuto,
    string? PluralParameter
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

        var jsonFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(Constants.AdditionalFilePattern))
            .Select((file, cancellationToken) => new
            {
                FileName = Path.GetFileName(file.Path),
                Content = file.GetText(cancellationToken)?.ToString()
            });

        var classWithJson = generationInfo.Combine(jsonFiles
            .Collect());

        context.RegisterSourceOutput(classWithJson, static (productionContext, data) =>
        {
            var ((hierarchy, info), jsonFiles) = data;

            if (jsonFiles.Length < 1)
                return;

            if (string.IsNullOrEmpty(info.InputFileName))
                return;

            string className = hierarchy.MetadataName;
            string namespaceName = hierarchy.Namespace;

            var config = ConfigRepository.Create(
                className: className,
                @namespace: namespaceName,
                baseLocale: string.IsNullOrEmpty(info.BaseLocale) ? "en" : info.BaseLocale,
                pluralParameter: string.IsNullOrEmpty(info.PluralParameter) ? "n" : info.PluralParameter,
                inputFileName: info.InputFileName
            );

            var paths = jsonFiles
                .Where(file => file.FileName.StartsWith(config.InputFileName));

            var fileCollection = FilesRepository.GetFileCollection(
                config.BaseLocale,
                allFiles: paths.Select(file => (file.FileName, file.Content))
            );

            _ = TranslationsCodeBuilder.Generate(productionContext, config, fileCollection);
        });
    }


    private static TranslationsParam ValidateTargetTypeAndGetInfo(AttributeData attributeData)
    {
        string? inputFileName = attributeData.GetNamedArgument<string>("InputFileName");
        string? baseLocale = attributeData.GetNamedArgument<string>("BaseLocale");
        string? pluralAuto = attributeData.GetNamedArgument<string>("PluralAuto");
        string? pluralParameter = attributeData.GetNamedArgument<string>("PluralParameter");

        return new TranslationsParam(
            InputFileName: inputFileName,
            BaseLocale: baseLocale,
            PluralAuto: pluralAuto,
            PluralParameter: pluralParameter
        );
    }
}