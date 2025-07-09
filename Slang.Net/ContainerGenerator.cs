namespace Slang;

[Generator]
#pragma warning disable RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
public partial class ContainerGenerator : DiagnosticAnalyzer
#pragma warning restore RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
{
    private void Execute(GeneratorContext context)
    {
        try
        {
            var roots = new ServiceProviderBuilder(context).BuildRoots();
        }
        catch (Exception e)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UnexpectedErrorDescriptor, Location.None,
                e.ToString().Replace(Environment.NewLine, " ")));
        }
    }

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
        {
            var syntaxCollector = new SyntaxCollector();
            compilationStartAnalysisContext.RegisterSyntaxNodeAction(
                analysisContext => { syntaxCollector.OnVisitSyntaxNode(analysisContext.Node); },
                SyntaxKind.ClassDeclaration, SyntaxKind.InterfaceDeclaration, SyntaxKind.InvocationExpression);

            compilationStartAnalysisContext.RegisterCompilationEndAction(compilationContext =>
            {
                Execute(new GeneratorContext(compilationContext, syntaxCollector));
            });
        });
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = new[]
    {
        DiagnosticDescriptors.UnexpectedErrorDescriptor,
        DiagnosticDescriptors.ServiceRequiredToConstructNotRegistered,
        DiagnosticDescriptors.MemberReferencedByInstanceOrFactoryAttributeNotFound,
        DiagnosticDescriptors.MemberReferencedByInstanceOrFactoryAttributeAmbiguous,
        DiagnosticDescriptors.ServiceProviderTypeHasToBePartial,
        DiagnosticDescriptors.ImportedTypeNotMarkedWithModuleAttribute,
        DiagnosticDescriptors.ImplementationTypeRequiresPublicConstructor,
        DiagnosticDescriptors.CyclicDependencyDetected,
        DiagnosticDescriptors.MissingServiceProviderAttribute,
        DiagnosticDescriptors.NoServiceTypeRegistered,
        DiagnosticDescriptors.ImplementationTypeAndFactoryNotAllowed,
        DiagnosticDescriptors.FactoryMemberMustBeAMethodOrHaveDelegateType,
        DiagnosticDescriptors.ServiceNameMustBeAlphanumeric,
        DiagnosticDescriptors.ImplicitIEnumerableNotNamed,
        DiagnosticDescriptors.BuiltInServicesAreNotNamed,
        DiagnosticDescriptors.NoServiceTypeAndNameRegistered,
        DiagnosticDescriptors.NamedServiceRequiredToConstructNotRegistered,
        DiagnosticDescriptors.OnlyStringKeysAreSupported,
        DiagnosticDescriptors.NullableServiceNotRegistered,
        DiagnosticDescriptors.NullableServiceRegistered,
    }.ToImmutableArray();

    private static string ReadAttributesFile()
    {
        using var manifestResourceStream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream("Slang.Attributes.cs");
        Debug.Assert(manifestResourceStream != null);
        using var reader = new StreamReader(manifestResourceStream);
        return reader.ReadToEnd();
    }
}