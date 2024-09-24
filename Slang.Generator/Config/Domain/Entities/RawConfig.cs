using System.Globalization;

namespace Slang.Generator.Config.Domain.Entities;

/// <summary>
/// Represents a slang.yaml file
/// </summary>
/// <param name="BaseLocale"></param>
/// <param name="FallbackStrategy"></param>
/// <param name="InputDirectory"></param>
/// <param name="InputFilePattern"></param>
/// <param name="PluralAuto"></param>
/// <param name="PluralParameter"></param>
public record RawConfig(
    CultureInfo BaseLocale,
    FallbackStrategy FallbackStrategy,
    string? InputDirectory,
    string InputFilePattern,
    PluralAuto PluralAuto,
    string PluralParameter
);