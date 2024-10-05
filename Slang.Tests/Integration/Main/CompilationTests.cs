using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Slang.Tests.Integration.EmbeddedLoader;

namespace Slang.Tests.Integration.Main;

public class CompilationTests
{
    [Test]
    public void Single_output_en()
    {
        string en = LoadResource("Slang.Tests.Integration.Resources._expected_en.output");
        string de = LoadResource("Slang.Tests.Integration.Resources._expected_de.output");
        string header = LoadResource("Slang.Tests.Integration.Resources._expected_header.output");

        ExpectSourceCode([
            CSharpSyntaxTree.ParseText(en),
            CSharpSyntaxTree.ParseText(de),
            CSharpSyntaxTree.ParseText(header)
        ]);
    }

    private static void ExpectSourceCode(SyntaxTree[] syntaxTrees)
    {
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
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) // Компилируем в динамическую библиотеку
        );

        var result = compilation.Emit(Stream.Null);

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            string errorMessages = string.Join(Environment.NewLine, failures.Select(f => f.ToString()));

            Assert.Fail($"Compilation failed with the following errors: {Environment.NewLine}{errorMessages}");
        }

        Assert.Pass("Compilation succeeded.");
    }
}