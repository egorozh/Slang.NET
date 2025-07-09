#if ROSLYN4_0_OR_GREATER
namespace Slang
{
#pragma warning disable RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
    public partial class ContainerGenerator : IIncrementalGenerator
#pragma warning restore RS1001 // We don't want this to be discovered as analyzer but it simplifies testing
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var attributedClasses = context.SyntaxProvider.CreateSyntaxProvider(
                (node, _) => SyntaxCollector.IsCandidateType(node),
                (syntaxContext, _) => (TypeDeclarationSyntax)syntaxContext.Node);

            var input = attributedClasses.Combine(context.CompilationProvider);

            context.RegisterSourceOutput(input, (productionContext, inputs) =>
                Execute(new GeneratorContext(
                    productionContext,
                    ImmutableArray.Create(inputs.Left),
                    inputs.Right)));

            context.RegisterPostInitializationOutput(c => { c.AddSource("Attributes.cs", ReadAttributesFile()); });
        }
    }
}

#endif