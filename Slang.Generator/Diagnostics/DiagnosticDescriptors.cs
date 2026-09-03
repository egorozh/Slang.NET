using Microsoft.CodeAnalysis;

namespace Slang.Generator.Diagnostics;

internal static class DiagnosticDescriptors
{
    /// <summary>
    /// Two or more translation files resolve to the same locale. Only the first one is generated,
    /// so the rest would be dropped without a trace.
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateLocale = new(
        id: "SLANG001",
        title: "Several translation files map to the same locale",
        messageFormat:
            "Translation files {0} all map to the locale '{1}'. Only '{2}' is used, the others are ignored.",
        category: "Slang",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
