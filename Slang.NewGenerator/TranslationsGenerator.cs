using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Slang.Generator.CodeBuilder;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;
using Slang.Generator.Extensions;
using Slang.Shared;

namespace Slang.Generator;

public record TranslationsParam(
    string? InputFileName,
    PluralAuto? PluralAuto,
    string? PluralParameter,
    string? RootPropertyName
);

public record struct ProjectParam(
    string? BaseCulture
);

internal sealed record HierarchyInfo(string MetadataName, string Namespace)
{
    public static HierarchyInfo From(INamedTypeSymbol typeSymbol) => new(
        typeSymbol.MetadataName,
        typeSymbol.ContainingNamespace.ToDisplayString(
            new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)));
}

internal sealed record Result<TValue>(TValue Value) where TValue : IEquatable<TValue>?;

[Generator(LanguageNames.CSharp)]
public sealed class TranslationsGenerator : IIncrementalGenerator
{
    private int _i;

    private const string AttributeFullName = "Slang.TranslationsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext ctx)
    {
        var jsonFilesProvider =
            ctx.AdditionalTextsProvider
                .Where(file => file.Path.EndsWith(Constants.AdditionalFilePattern))
                .Select((file, cancellationToken) => new
                {
                    FileName = Path.GetFileName(file.Path),
                    Content = file.GetText(cancellationToken)?.ToString()
                });

        var configFileProvider = ctx.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("slang.json"))
            .Select((file, ct) =>
            {
                string? jsonText = file.GetText(ct)?.ToString();

                if (jsonText != null)
                {
                    var config = JsonSerializer.Deserialize<GlobalConfigDto>(jsonText);

                    return new ProjectParam(BaseCulture: config?.BaseCulture);
                }

                return new ProjectParam(BaseCulture: null);
            });

        var attributedClasses =
            ctx.SyntaxProvider
                .ForAttributeWithMetadataName(
                    AttributeFullName,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, token) =>
                    {
                        var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                        // Gather all generation info, and any diagnostics
                        var info = ValidateTargetTypeAndGetInfo(ctx.Attributes[0]);

                        token.ThrowIfCancellationRequested();

                        var hierarchy = HierarchyInfo.From(typeSymbol);

                        token.ThrowIfCancellationRequested();

                        return new Result<(HierarchyInfo, TranslationsParam?)>((hierarchy, info));
                    });

        var input = attributedClasses
            .Combine(configFileProvider.Collect())
            .Combine(jsonFilesProvider.Collect());

        ctx.RegisterSourceOutput(
            jsonFilesProvider,
            (productionContext, data) =>
            {
                productionContext.AddSource($"Translations{_i++}.g.cs",
                    SourceText.From(
                        "error + \" | \" + jsonFile.FileName",
                        Encoding.UTF8));

                // foreach (var jsonFile in jsonFiles)
                // {
                //     productionContext.AddSource($"Translations{_i++}.g.cs",
                //         SourceText.From(
                //             error + " | " + jsonFile.FileName,
                //             Encoding.UTF8));
                // }

                // var jsonFiles = data.Right;
                // var projectParams = data.Left.Right;
                //
                // var attrClasses = data.Left.Left;
                //
                // var (hierarchy, info) = attrClasses.Value;
                //
                // if (jsonFiles.Length < 1)
                //     return;
                //
                // if (string.IsNullOrEmpty(info?.InputFileName))
                //     return;
                //
                // string className = hierarchy.MetadataName;
                // string namespaceName = hierarchy.Namespace;
                //
                // var globalConfig = projectParams.FirstOrDefault();
                //
                // string? pluralParameter = string.IsNullOrEmpty(info.PluralParameter) ? "n" : info.PluralParameter;
                // string? rootPropertyName = string.IsNullOrEmpty(info.RootPropertyName) ? "Root" : info.RootPropertyName;
                // string? baseLocale = string.IsNullOrEmpty(globalConfig.BaseCulture) ? "en" : globalConfig.BaseCulture;
                //
                // string error = info.InputFileName is null
                //     ? "info null"
                //     : $"{className} - {namespaceName} - {baseLocale} - {info.InputFileName} - {pluralParameter} - {rootPropertyName}";
                //
                // var config = new RawConfig(
                //     Namespace: namespaceName,
                //     ClassName: className,
                //     BaseLocale: new CultureInfo(baseLocale),
                //     InputFileName: info.InputFileName!,
                //     PluralAuto: PluralAuto.Cardinal,
                //     PluralParameter: pluralParameter,
                //     RootPropertyName: rootPropertyName
                // );
                //
                //
                // var paths = jsonFiles
                //     .Where(file => file.FileName.StartsWith(config.InputFileName));
                //
                // var fileCollection = FilesRepository.GetFileCollection(
                //     config.BaseLocale,
                //     allFiles: paths.Select(file => (file.FileName, file.Content!))
                // );

                //_ = TranslationsCodeBuilder.Generate(productionContext, config, fileCollection);
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