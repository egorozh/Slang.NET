using System.Globalization;
using System.Text.RegularExpressions;
using Slang.Generator.Config.Entities;

namespace Slang.Generator.Files;

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
    string Path,
    Func<Task<string>> Read,
    CultureInfo Locale
);

public static partial class FilesRepository
{
    private static readonly Regex FileWithLocaleRegex = MyFileWithLocaleRegex();
    private static readonly Regex BaseFileRegex = MyBaseFileRegex();

    
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

    
    private static TranslationFile? GetTranslationFile(RawConfig config, FileInfo f)
    {
        string fileNameNoExtension = Path.GetFileNameWithoutExtension(f.Name).Split('.').First();

        var baseFileMatch = BaseFileRegex.Match(fileNameNoExtension);

        if (baseFileMatch.Success)
        {
            // base file (file without locale, may be multiples due to namespaces!)
            // could also be a non-base locale when directory name is a locale

            return new TranslationFile(
                Path: f.FullName,
                Locale: config.BaseLocale,
                Read: () => File.ReadAllTextAsync(f.FullName));
        }

        // secondary files (strings_x)
        var match = FileWithLocaleRegex.Match(fileNameNoExtension);

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
                Path: f.FullName,
                Locale: locale,
                Read: () => File.ReadAllTextAsync(f.FullName));
        }

        return null;
    }


    /// Finds the parts of the locale. It must start with an underscore.
    /// groups for strings-zh-Hant-TW:
    /// 1 = strings
    /// 2 = zh (language, non-nullable)
    /// 3 = Hant (script)
    /// 4 = TW (country)
    [GeneratedRegex("^(?:([a-zA-Z0-9]+)[_-])?([a-z]{2,3})(?:[_-]([A-Za-z]{4}))?(?:[_-]([A-Z]{2}|[0-9]{3}))?$")]
    private static partial Regex MyFileWithLocaleRegex();

    /// matches any string without special characters
    [GeneratedRegex("^([a-zA-Z0-9]+)?$")]
    private static partial Regex MyBaseFileRegex();
}