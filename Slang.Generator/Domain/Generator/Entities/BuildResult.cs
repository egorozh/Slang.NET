using System.Globalization;

namespace Slang.Generator.Domain.Generator.Entities;

/// <summary>
/// The resulting output strings
/// It can either be rendered as a single file
/// or as multiple files.
/// </summary>
public record BuildResult(
    string Header,
    Dictionary<CultureInfo, string> Translations
);