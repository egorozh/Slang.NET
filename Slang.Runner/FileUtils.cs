using System.Text.Json;

namespace Slang.Runner;

public static class FileUtils
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    const string INFO_KEY = "@@info";

    public static void writeFile(string path, string content)
    {
        File.WriteAllText(path, content);
    }

    public static void WriteFileOfType(string path, Dictionary<string, dynamic> content)
    {
        writeFile(path, EncodeContent(content));
    }

    private static string EncodeContent(Dictionary<string, object> content)
    {
        // this encoder does not append \n automatically
        //return $"{JsonEncoder.withIndent(" ").convert(content)}\n";
        return JsonSerializer.Serialize(content, Options);
    }

    public static void createMissingFolders(string filePath)
    {
        int index = filePath
            .Replace('/', Path.PathSeparator)
            .Replace('\\', Path.PathSeparator)
            .LastIndexOf(Path.PathSeparator);

        if (index == -1)
            return;

        string directoryPath = filePath[..index];

        new DirectoryInfo(directoryPath).Create();
    }
}