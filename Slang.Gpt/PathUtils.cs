namespace Slang.Gpt;

/// Operations on paths
public static class PathUtils
{
    /// converts /some/path to /some/path/my_file.json
    public static string WithFileName(
        string directoryPath,
        string fileName,
        char pathSeparator
    )
    {
        if (directoryPath.EndsWith(pathSeparator))
            return directoryPath + fileName;

        return directoryPath + pathSeparator + fileName;
    }
}