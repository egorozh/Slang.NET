using System.Globalization;

namespace Slang.Utilities.Core.Translate;

/// <summary>
/// A collection of translation files that can be read in a later step.
/// This is an abstraction to support build_runner and the custom CLI by
/// providing a common [FileReader] interface.
/// </summary>
public record struct SlangFileCollection(List<TranslationFile> Files);

/// <param name="Locale">The inferred locale of this file (by file name, directory name, or config)</param>
public record struct TranslationFile(
    Func<Task<string>> Read,
    string FileName,
    string FilePath,
    CultureInfo Locale,
    string Namespace
);