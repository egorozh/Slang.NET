using System.Globalization;

namespace Slang.Generator.Core.Entities;

/// <summary>
/// Represents a input parameters
/// </summary>
public record RawConfig(
    CultureInfo BaseLocale,
    string InputFileName,
    string Namespace,
    string ClassName,
    PluralAuto PluralAuto,
    string PluralParameter,
    string RootPropertyName
);