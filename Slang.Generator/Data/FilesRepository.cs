using System.Globalization;
using Slang.Generator.Domain.Entities;

namespace Slang.Generator.Data;

/// <summary>
/// A collection of translation files that can be read in a later step.
/// This is an abstraction to support build_runner and the custom CLI by
/// providing a common [FileReader] interface.
/// </summary>
public record struct SlangFileCollection(
    RawConfig Config,
    List<TranslationFile> Files
);

/// <param name="Locale">The inferred locale of this file (by file name, directory name, or config)</param>
public record struct TranslationFile(
    Func<Task<string>> Read,
    CultureInfo Locale
);

public static class FilesRepository
{
    public static SlangFileCollection GetFileCollection(RawConfig config, IReadOnlyList<FileInfo> allFiles)
    {
        var files = allFiles
            .Select(f => GetTranslationFile(config, f))
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .OrderBy(file => $"{file.Locale}")
            .ToList();

        return new SlangFileCollection(
            Config: config,
            Files: files
        );
    }

    public static SlangFileCollection GetFileCollection(RawConfig config, IEnumerable<(string, string)> allFiles)
    {
        var files = allFiles
            .Select(f => GetTranslationFile(config, f.Item1, f.Item2))
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .OrderBy(file => $"{file.Locale}")
            .ToList();

        return new SlangFileCollection(
            Config: config,
            Files: files
        );
    }

    private static TranslationFile? GetTranslationFile(RawConfig config, FileInfo f)
    {
        string fileNameNoExtension = Path.GetFileNameWithoutExtension(f.Name).Split('.').First();

        var baseFileMatch = Regexes.BaseFileRegex.Match(fileNameNoExtension);

        if (baseFileMatch.Success)
        {
            // base file (file without locale, may be multiples due to namespaces!)
            // could also be a non-base locale when directory name is a locale

            return new TranslationFile(
                Locale: config.BaseLocale,
                Read: () => File.ReadAllTextAsync(f.FullName));
        }

        // secondary files (strings_x)
        var match = Regexes.FileWithLocaleRegex.Match(fileNameNoExtension);

        if (match.Success)
        {
            string language = match.Groups[2].Value;

            //todo: scriptCode not supported
            string script = match.Groups[3].Value;

            string country = match.Groups[4].Value;

            var locale = new CultureInfo(string.IsNullOrWhiteSpace(country)
                ? $"{language}"
                : $"{language}-{country}");

            return new TranslationFile(
                Locale: locale,
                Read: () => File.ReadAllTextAsync(f.FullName));
        }

        return null;
    }

    private static TranslationFile? GetTranslationFile(RawConfig config, string fileName, string content)
    {
        string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName).Split('.').First();

        var baseFileMatch = Regexes.BaseFileRegex.Match(fileNameNoExtension);

        if (baseFileMatch.Success)
        {
            // base file (file without locale, may be multiples due to namespaces!)
            // could also be a non-base locale when directory name is a locale

            return new TranslationFile(
                Locale: config.BaseLocale,
                Read: () => Task.FromResult(content));
        }

        // secondary files (strings_x)
        var match = Regexes.FileWithLocaleRegex.Match(fileNameNoExtension);

        if (match.Success)
        {
            string language = match.Groups[2].Value;

            //todo: scriptCode not supported
            string script = match.Groups[3].Value;

            string country = match.Groups[4].Value;

            var locale = new CultureInfo(string.IsNullOrWhiteSpace(country)
                ? $"{language}"
                : $"{language}-{country}");

            return new TranslationFile(
                Locale: locale,
                Read: () => Task.FromResult(content));
        }

        return null;
    }
}