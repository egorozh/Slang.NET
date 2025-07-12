namespace Slang;
#pragma warning disable RS2000
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnexpectedErrorDescriptor = new("SLANG0001",
        "Unexpected error during generation",
        "Unexpected error occurred during code generation: {0}", "Usage", DiagnosticSeverity.Error, true);
}