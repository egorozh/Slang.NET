using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Slang.Tests.Helpers;

internal static class CompilationAssert
{
    /// <summary>
    /// Compiles the given generated sources and fails the test on any compiler error.
    /// </summary>
    public static void Compiles(params string[] sources)
    {
        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source)).ToArray();

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var assembly = typeof(PluralResolver).Assembly;

        references.Add(MetadataReference.CreateFromFile(assembly.Location));

        var compilation = CSharpCompilation.Create(
            "Slang.Tests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        var result = compilation.Emit(Stream.Null);

        if (result.Success)
            return;

        var failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);

        string errorMessages = string.Join(Environment.NewLine, failures.Select(f => f.ToString()));

        Assert.Fail($"Compilation failed with the following errors: {Environment.NewLine}{errorMessages}");
    }
}
