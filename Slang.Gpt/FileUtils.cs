using System.Text.Json;
using System.Text.RegularExpressions;

namespace Slang.Gpt;

public static partial class FileUtils
{
    private static readonly Regex UnicodeRegex = MyUnicodeRegex();

    public static void WriteFileOfType(string path, Dictionary<string, object> content)
    {
        string jsonFromContext = JsonSerializer.Serialize(content, DictionaryContext.Default.DictionaryStringObject);

        string EscapeNonAsciiCharacters(string json)
        {
            return UnicodeRegex.Replace(json, match =>
            {
                string hexValue = match.Groups["Value"].Value;
                int unicode = Convert.ToInt32(hexValue, 16);
                string symbol = ((char)unicode).ToString();

                return symbol.Replace("\"", "\\\"");
            });
        }

        string escapedJson = EscapeNonAsciiCharacters(jsonFromContext);

        File.WriteAllText(path, escapedJson);
    }

    [GeneratedRegex(@"\\u(?<Value>[a-fA-F0-9]{4})")]
    private static partial Regex MyUnicodeRegex();
}