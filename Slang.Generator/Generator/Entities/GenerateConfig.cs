using System.Globalization;

namespace Slang.Generator.Generator.Entities;

/// <summary>
/// Config for the generation step (generate dart-content from model)
/// Applies to all locales
/// </summary>
internal record GenerateConfig(
    string Namespace,
    string ClassName,
    CultureInfo BaseLocale
);