using System.Globalization;

namespace Slang.Runner;

/// Operations on paths
public static class PathUtils
{
    /// converts /some/path/file.json to file
    public static string GetFileNameNoExtension(string path)
    {
        return GetFileName(path).Split('.').First();
    }

    /// converts /some/path/file.i18n.json to i18n.json
    public static string GetFileExtension(string path)
    {
        string fileName = GetFileName(path);
        int firstDot = fileName.LastIndexOf('.');
        return fileName[(firstDot + 1)..];
    }

    /// converts /a/b/file.json to [a, b, file.json]
    public static List<string> GetPathSegments(string path)
    {
        return path
            .Replace('\\', '/')
            .Split('/')
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    /// converts /some/path/file.json to /some/path/newFile.json
    public static string ReplaceFileName(
        string path,
        string newFileName,
        string pathSeparator
    )
    {
        int index = path.LastIndexOf(pathSeparator);

        if (index == -1)
            return newFileName;

        return path[..(index + pathSeparator.Length)] + newFileName;
    }

    /// converts /some/path/file.json to file.json
    private static string GetFileName(string path)
    {
        return path.Replace('\\', '/').Split('/').Last();
    }
}

public static class BuildResultPaths
{
    public static String MainPath(String outputPath)
    {
        //todo: getting input info from NET SOURCE GENERATOR
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        return Path.Combine(targetDirectory, outputPath);
    }

    public static String LocalePath(string outputPath, CultureInfo locale)
    {
        //todo: getting input info from NET SOURCE GENERATOR
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        string fileNameNoExtension = PathUtils.GetFileNameNoExtension(outputPath);
        string localeExt = locale.TwoLetterISOLanguageName.Replace('-', '_');
        string fileName = $"{fileNameNoExtension}_{localeExt}.g.cs";
        return Path.Combine(targetDirectory, PathUtils.ReplaceFileName(
            path: outputPath,
            newFileName: fileName,
            pathSeparator: "/"
        ));
    }
}