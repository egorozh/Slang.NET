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
        if (TranslationFileNames.Parse(fileName) is not { } parsed)
            return null;

        if (parsed.LocaleTag is not { } localeTag)
        {
            // base file (file without locale, may be multiples due to namespaces!)
            // could also be a non-base locale when directory name is a locale
            return new TranslationFile(
                Locale: baseCulture,
                Namespace: parsed.Namespace,
                FileName: fileName,
                FilePath: filePath,
                Read: contentFactory);
        }

        if (!TranslationFileNames.TryCreateCulture(localeTag, out var locale))
            return null;

        return new TranslationFile(
            Locale: locale,
            Namespace: parsed.Namespace,
            FileName: fileName,
            FilePath: filePath,
            Read: contentFactory);
    }
}
