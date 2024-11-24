using System.Globalization;
using Slang.Shared;

namespace Slang.Generator.Data;

/// <summary>
/// A collection of translation files that can be read in a later step.
/// This is an abstraction to support build_runner and the custom CLI by
/// providing a common [FileReader] interface.
/// </summary>
public record struct SlangFileCollection(List<TranslationFile> Files);

/// <param name="Locale">The inferred locale of this file (by file name, directory name, or config)</param>
public record struct TranslationFile(
    Func<Task<string>> Read,
    CultureInfo Locale
);

public static class FilesRepository
{
    public static SlangFileCollection GetFileCollection(CultureInfo baseCulture, IReadOnlyList<FileInfo> allFiles)
    {
        var files = allFiles
            .Select(f => GetTranslationFile(baseCulture, f))
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .OrderBy(file => $"{file.Locale}")
            .ToList();

        return new SlangFileCollection(Files: files);
    }

    public static SlangFileCollection GetFileCollection(
        CultureInfo baseCulture,
        IEnumerable<(string FileName, string Content)> allFiles)
    {
        var files = allFiles
            .Select(f => GetTranslationFile(baseCulture, f.FileName, f.Content))
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .OrderBy(file => $"{file.Locale}")
            .ToList();

        return new SlangFileCollection(Files: files);
    }

    private static TranslationFile? GetTranslationFile(CultureInfo baseCulture, FileInfo f)
    {
        return GetTranslationFile(baseCulture, f.Name, () => ReadFileContentAsync(f.FullName));
    }

    private static TranslationFile? GetTranslationFile(CultureInfo baseCulture, string fileName, string content)
    {
        return GetTranslationFile(baseCulture, fileName, () => Task.FromResult(content));
    }

    internal static TranslationFile? GetTranslationFile(CultureInfo baseCulture, string fileName,
        Func<Task<string>> contentFactory)
    {
        string fileNameNoExtension = Path.GetFileNameWithoutExtension(fileName).Split('.').First();

        var baseFileMatch = Regexes.BaseFileRegex.Match(fileNameNoExtension);

        if (baseFileMatch.Success)
        {
            // base file (file without locale, may be multiples due to namespaces!)
            // could also be a non-base locale when directory name is a locale

            return new TranslationFile(
                Locale: baseCulture,
                Read: contentFactory);
        }

        // secondary files (strings_x)
        var match = Regexes.FileWithLocaleRegex.Match(fileNameNoExtension);

        if (match.Success)
        {
            string language = match.Groups[2].Value;

            //todo: scriptCode not supported
            string _ = match.Groups[3].Value;

            string country = match.Groups[4].Value;

            var locale = new CultureInfo(string.IsNullOrWhiteSpace(country)
                ? $"{language}"
                : $"{language}-{country}");

            return new TranslationFile(
                Locale: locale,
                Read: contentFactory);
        }

        return null;
    }

    private static async Task<string> ReadFileContentAsync(string fileName)
    {
        await using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            FileOptions.Asynchronous);
        
        using var reader = new StreamReader(stream);
        
        string content = await reader.ReadToEndAsync();

        return content;
    }
}