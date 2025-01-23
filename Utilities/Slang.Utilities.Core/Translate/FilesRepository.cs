using System.Globalization;
using Slang.Shared;

namespace Slang.Utilities.Core.Translate;

internal static class FilesRepository
{
    public static SlangFileCollection GetFileCollection(CultureInfo baseCulture, IEnumerable<FileInfo> allFiles)
    {
        var files = allFiles
            .Select(f => GetTranslationFile(baseCulture, f))
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .OrderBy(file => $"{file.Locale}")
            .ToList();

        return new SlangFileCollection(Files: files);
    }

    private static TranslationFile? GetTranslationFile(CultureInfo baseCulture, FileInfo f)
    {
        return GetTranslationFile(baseCulture, f.Name, f.FullName, () => File.ReadAllTextAsync(f.FullName));
    }

    private static TranslationFile? GetTranslationFile(CultureInfo baseCulture, string fileName,
        string filePath,
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
                Namespace: fileName.Replace(Constants.AdditionalFilePattern, ""),
                FileName: fileName,
                FilePath: filePath,
                Read: contentFactory);
        }

        // secondary files (strings_x)
        var match = Regexes.FileWithLocaleRegex.Match(fileNameNoExtension);

        if (match.Success)
        {
            string @namespace = match.Groups[1].Value;

            string language = match.Groups[2].Value;

            //todo: scriptCode not supported
            string _ = match.Groups[3].Value;

            string country = match.Groups[4].Value;

            var locale = new CultureInfo(string.IsNullOrWhiteSpace(country)
                ? $"{language}"
                : $"{language}-{country}");

            return new TranslationFile(
                Locale: locale,
                Namespace: @namespace,
                FileName: fileName,
                FilePath: filePath,
                Read: contentFactory);
        }

        return null;
    }
}