using Slang.Shared;

namespace Slang.Gpt.Domain.Utils;

public static class FileUtils
{
    public static void WriteFileOfType(string path, Dictionary<string, object?> content)
    {
        string jsonFromContext = JsonHelpers.JsonEncode(content);

        string EscapeNonAsciiCharacters(string json)
        {
            return Regexes.UnicodeRegex.Replace(json, match =>
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
}