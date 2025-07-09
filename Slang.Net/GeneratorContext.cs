namespace Slang;

internal readonly struct GeneratorContext
{
#if ROSLYN4_0_OR_GREATER
    private readonly SourceProductionContext? _sourceProductionContext;

    public GeneratorContext(
        SourceProductionContext sourceProductionContext,
        ImmutableArray<TypeDeclarationSyntax> candidateTypes,
        Compilation compilation)
    {
        _sourceProductionContext = sourceProductionContext;
        CandidateTypes = candidateTypes;
        Compilation = compilation;
        _compilationAnalysisContext = null;
    }
#else
        private readonly GeneratorExecutionContext? _sourceProductionContext;

        public GeneratorContext(GeneratorExecutionContext sourceProductionContext)
        {
            _sourceProductionContext = sourceProductionContext;

            var syntaxCollector = (SyntaxCollector)sourceProductionContext.SyntaxReceiver!;
           
            CandidateTypes = syntaxCollector.CandidateTypes;
            Compilation = sourceProductionContext.Compilation;
            _compilationAnalysisContext = null;
        }
#endif

    private readonly CompilationAnalysisContext? _compilationAnalysisContext;

    public GeneratorContext(CompilationAnalysisContext compilationAnalysisContext, SyntaxCollector syntaxCollector)
    {
        _compilationAnalysisContext = compilationAnalysisContext;
        
        CandidateTypes = syntaxCollector.CandidateTypes;
        Compilation = compilationAnalysisContext.Compilation;
        _sourceProductionContext = null;
    }

    public Compilation Compilation { get; }
    
    public IEnumerable<TypeDeclarationSyntax> CandidateTypes { get; }

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        _sourceProductionContext?.ReportDiagnostic(diagnostic);
        _compilationAnalysisContext?.ReportDiagnostic(diagnostic);
    }

    public void AddSource(string nameHint, string code)
    {
        _sourceProductionContext?.AddSource(nameHint, code);
    }
}