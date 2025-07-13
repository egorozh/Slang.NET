using System.Globalization;
using Microsoft.CodeAnalysis.Text;
using Slang.Generator.CodeBuilder;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;
using Slang.Generator.Extensions;

namespace Slang;

[Generator]
#pragma warning disable RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
public partial class TranslationsGenerator
    //: DiagnosticAnalyzer
#pragma warning restore RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
{
    private void Execute(GeneratorContext context)
    {
        try
        {
            var hierarchy = context.Result.HierarchyInfo;
            var info = context.Result.Params;

            //var (hierarchy, info) = attrClasses.Value;
            string className = hierarchy.MetadataName;
            string namespaceName = hierarchy.Namespace;

            if (context.JsonFiles.Length < 1)
                return;

            if (string.IsNullOrEmpty(info?.InputFileName))
                return;

            var globalConfig = context.ProjectParams.FirstOrDefault();

            string pluralParameter = string.IsNullOrEmpty(info?.PluralParameter) ? "n" : info!.PluralParameter!;
            string rootPropertyName = string.IsNullOrEmpty(info?.RootPropertyName) ? "Root" : info!.RootPropertyName!;
            string? baseLocale = string.IsNullOrEmpty(globalConfig.BaseCulture)
                ? "en"
                : globalConfig.BaseCulture;

            string error = info?.InputFileName is null
                ? "info null"
                : $"{className} - {namespaceName} - {baseLocale} - {info.InputFileName} - {pluralParameter} - {rootPropertyName}";

            var config = new RawConfig(
                Namespace: namespaceName,
                ClassName: className,
                BaseLocale: new CultureInfo(baseLocale),
                InputFileName: info?.InputFileName!,
                PluralAutoEntity: PluralAutoEntity.Cardinal,
                PluralParameter: pluralParameter,
                RootPropertyName: rootPropertyName
            );
            //

            var paths = context.JsonFiles
                .Where(file => file.FileName.StartsWith(config.InputFileName));

            var fileCollection = FilesRepository.GetFileCollection(
                config.BaseLocale,
                allFiles: paths.Select(file => (file.FileName, file.Content!))
            );

            // foreach (var jsonFile in context.JsonFiles)
            // {
            //     context.AddSource($"Translations{_i++}.g.cs",
            //         error + baseLocale + " | " + jsonFile.FileName);
            // }

            _ = TranslationsCodeBuilder.Generate(context, config, fileCollection);
        }
        catch (Exception)
        {
            // context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnexpectedErrorDescriptor, Location.None,
            //     e.ToString().Replace(Environment.NewLine, " ")));
        }
    }

    // public override void Initialize(AnalysisContext context)
    // {
    //     context.EnableConcurrentExecution();
    //     // context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
    //     //                                        GeneratedCodeAnalysisFlags.ReportDiagnostics);
    //     // context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
    //     // {
    //     //     var syntaxCollector = new SyntaxCollector();
    //     //     compilationStartAnalysisContext.RegisterSyntaxNodeAction(
    //     //         analysisContext => { syntaxCollector.OnVisitSyntaxNode(analysisContext.Node); },
    //     //         SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.InvocationExpression);
    //     //
    //     //     // compilationStartAnalysisContext.RegisterCompilationEndAction(compilationContext =>
    //     //     // {
    //     //     //     //Execute(new GeneratorContext(compilationContext, syntaxCollector));
    //     //     // });
    //     // });
    // }

    // public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    // [
    //     DiagnosticDescriptors.UnexpectedErrorDescriptor,
    // ];

    private static string ReadAttributesFile()
    {
        using var manifestResourceStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream("Slang.Attributes.cs");
        Debug.Assert(manifestResourceStream != null);
        using var reader = new StreamReader(manifestResourceStream);
        return reader.ReadToEnd();
    }

    private static TranslationsParam ValidateTargetTypeAndGetInfo(AttributeData attributeData)
    {
        string? inputFileName = attributeData.GetNamedArgument<string>("InputFileName");
        var pluralAuto = attributeData.GetNamedArgument<PluralAutoEntity?>("PluralAuto");
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