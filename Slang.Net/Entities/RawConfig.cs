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
    PluralAutoEntity PluralAutoEntity,
    string PluralParameter,
    string RootPropertyName,
    string StartCharacter,
    string EndCharacter
);