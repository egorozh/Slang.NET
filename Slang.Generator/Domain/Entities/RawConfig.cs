using System.Globalization;

namespace Slang.Generator.Domain.Entities;

/// <summary>
/// Represents a input parameters
/// </summary>
public record RawConfig(
    CultureInfo BaseLocale,
    string InputFileName,
    string Namespace,
    string ClassName,
    string? InputDirectory,
    string InputFilePattern,
    PluralAuto PluralAuto,
    string PluralParameter
);