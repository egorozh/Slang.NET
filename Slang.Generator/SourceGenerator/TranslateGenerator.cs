using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Slang.Generator.Data;
using Slang.Generator.Domain.Entities;
using Slang.Generator.SourceGenerator.CodeBuilder;
using Slang.Generator.SourceGenerator.Extensions;
using Slang.Generator.SourceGenerator.Models;
using Slang.Shared;

namespace Slang.Generator.SourceGenerator;

public record TranslationsParam(
    string? InputFileName,
    PluralAuto? PluralAuto,
    string? PluralParameter,
    string? RootPropertyName
);

public record struct ProjectParam(
    string? BaseCulture
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
                    (ctx, token) =>
                    {
                        INamedTypeSymbol typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                        // Gather all generation info, and any diagnostics
                        TranslationsParam info = ValidateTargetTypeAndGetInfo(ctx.Attributes[0]);

                        token.ThrowIfCancellationRequested();

                        // If there are any diagnostics, there's no need to compute the hierarchy info at all, just return them

                        HierarchyInfo hierarchy = HierarchyInfo.From(typeSymbol);

                        token.ThrowIfCancellationRequested();

                        return new Result<(HierarchyInfo, TranslationsParam?)>((hierarchy, info));
                    });

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

        var configFile = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("slang.json"))
            .Select((file, ct) =>
            {
                string? jsonText = file.GetText(ct)?.ToString();

                if (jsonText != null)
                {
                    var config = JsonSerializer.Deserialize(jsonText, GlobalConfigContext.Default.GlobalConfigDto);

                    return new ProjectParam(BaseCulture: config?.BaseCulture);
                }

                return new ProjectParam(BaseCulture: null);
            });

        var generalProvider = generationInfo
            .Combine(jsonFiles.Collect())
            .Combine(configFile.Collect());

        context.RegisterSourceOutput(generalProvider, static (productionContext, data) =>
        {
            var (((hierarchy, info), jsonFiles), projectParams) = data;

            if (jsonFiles.Length < 1)
                return;

            if (string.IsNullOrEmpty(info.InputFileName))
                return;

            string className = hierarchy.MetadataName;
            string namespaceName = hierarchy.Namespace;

            var globalConfig = projectParams.FirstOrDefault();

            var config = ConfigRepository.Create(
                className: className,
                @namespace: namespaceName,
                baseLocale: string.IsNullOrEmpty(globalConfig.BaseCulture) ? "en" : globalConfig.BaseCulture,
                pluralAuto: info.PluralAuto ?? PluralAuto.Cardinal,
                pluralParameter: string.IsNullOrEmpty(info.PluralParameter) ? "n" : info.PluralParameter,
                rootPropertyName: string.IsNullOrEmpty(info.RootPropertyName) ? "Root" : info.RootPropertyName,
                inputFileName: info.InputFileName
            );

            var paths = jsonFiles
                .Where(file => file.FileName.StartsWith(config.InputFileName));

            var fileCollection = FilesRepository.GetFileCollection(
                config.BaseLocale,
                allFiles: paths.Select(file => (file.FileName, file.Content!))
            );

            // debug source
            //productionContext.AddSource($"{config.ClassName}.debug.g.cs", $"{info}");

            _ = TranslationsCodeBuilder.Generate(productionContext, config, fileCollection);
        });
    }

    private static TranslationsParam ValidateTargetTypeAndGetInfo(AttributeData attributeData)
    {
        string? inputFileName = attributeData.GetNamedArgument<string>("InputFileName");
        var pluralAuto = attributeData.GetNamedArgument<PluralAuto?>("PluralAuto");
        string? pluralParameter = attributeData.GetNamedArgument<string>("PluralParameter");
        string? rootPropertyName = attributeData.GetNamedArgument<string>("RootPropertyName");

        return new TranslationsParam(
            InputFileName: inputFileName,
            PluralAuto: pluralAuto,
            PluralParameter: pluralParameter,
            RootPropertyName: rootPropertyName
        );
    }
}

internal record GlobalConfigDto
{
    [JsonPropertyName("base_culture")] public string? BaseCulture { get; set; }
}

[JsonSerializable(typeof(GlobalConfigDto))]
internal partial class GlobalConfigContext : JsonSerializerContext;