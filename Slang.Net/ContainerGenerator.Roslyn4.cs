using System.Text.Json;
using System.Text.Json.Serialization;
using Slang.Generator.Core.Entities;
using Slang.Shared;

#if ROSLYN4_0_OR_GREATER
namespace Slang
{
    public record TranslationsParam(
        string? InputFileName,
        PluralAutoEntity? PluralAuto,
        string? PluralParameter,
        string? RootPropertyName
    );

    public record struct ProjectParam(
        string? BaseCulture
    );

    public record struct JsonFile(
        string FileName,
        string? Content
    );

    internal sealed record HierarchyInfo(string MetadataName, string Namespace)
    {
        public static HierarchyInfo From(INamedTypeSymbol typeSymbol) => new(
            typeSymbol.MetadataName,
            typeSymbol.ContainingNamespace.ToDisplayString(
                new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)));
    }

    internal sealed record Result(TranslationsParam? Params, HierarchyInfo HierarchyInfo);


#pragma warning disable RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
    public partial class ContainerGenerator : IIncrementalGenerator
#pragma warning restore RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
    {
        private const string AttributeFullName = "Slang.TranslationsAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            // var attributedClasses = ctx.SyntaxProvider.CreateSyntaxProvider(
            //     (node, _) => SyntaxCollector.IsCandidateType(node),
            //     (syntaxContext, _) => (TypeDeclarationSyntax)syntaxContext.Node);

            var attributedClasses =
                ctx.SyntaxProvider
                    .ForAttributeWithMetadataName(
                        AttributeFullName,
                        static (node, _) => node is ClassDeclarationSyntax,
                        (ctx, token) =>
                        {
                            var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                            // Gather all generation info, and any diagnostics
                            var info = ValidateTargetTypeAndGetInfo(ctx.Attributes[0]);

                            token.ThrowIfCancellationRequested();

                            var hierarchy = HierarchyInfo.From(typeSymbol);

                            token.ThrowIfCancellationRequested();

                            return new Result(info, hierarchy);
                        });

            var jsonFilesProvider =
                ctx.AdditionalTextsProvider
                    .Where(file => file.Path.EndsWith(Constants.AdditionalFilePattern))
                    .Select((file, cancellationToken) => new JsonFile
                    (
                        FileName: Path.GetFileName(file.Path),
                        Content: file.GetText(cancellationToken)?.ToString()
                    ));

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


            var input = attributedClasses
                .Combine(configFileProvider.Collect())
                .Combine(jsonFilesProvider.Collect())
                .Combine(ctx.CompilationProvider);

            ctx.RegisterSourceOutput(
                input,
                (productionContext, data) =>
                {
                    var compilation = data.Right;
                    var jsonFiles = data.Left.Right;
                    var projectParams = data.Left.Left.Right;
                    var syntax = data.Left.Left.Left;

                    Execute(new GeneratorContext(productionContext,
                        syntax,
                        jsonFiles,
                        projectParams,
                        compilation
                    ));
                });

            // context.RegisterSourceOutput(input, (productionContext, inputs) =>
            // {
            //     // Execute(new GeneratorContext(
            //     //     productionContext,
            //     //     ImmutableArray.Create(inputs.Left),
            //     //     inputs.Right));
            // });

            ctx.RegisterPostInitializationOutput(c => { c.AddSource("Attributes.cs", ReadAttributesFile()); });
        }
    }
}

#endif

internal record GlobalConfigDto
{
    [JsonPropertyName("base_culture")] public string? BaseCulture { get; set; }
}