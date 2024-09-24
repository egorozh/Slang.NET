using Slang.Generator.Config.Domain.Entities;

namespace Slang.Generator.Nodes.Data;

public static class StringExtensions
{
    /// de-DE will be interpreted as [de,DE]
    /// normally, it would be [de,D,E] which we do not want
    public static string ToCaseOfLocale(this string s, CaseStyle style) => s.ToLower().ToCase(style);

    /// transforms the string to the specified case
    /// if case is null, then no transformation will be applied
    public static string ToCase(this string s, CaseStyle? style)
    {
        switch (style)
        {
            case CaseStyle.Camel:
                return string.Join(string.Empty, s.GetWords()
                    .Select((word, index) => index == 0 ? word.ToLower() : word.Capitalize()));
            case CaseStyle.Pascal:
                return string.Join(string.Empty, s.GetWords().Select(word => word.Capitalize()));
            case null:
                return s;
            default:
                Console.WriteLine($"Unknown case: {style}");
                return s;
        }
    }

    /// transforms the string to the specified case
    /// if case is null, then no transformation will be applied
    public static string ToCaseWithDots(this string s, CaseStyle? style)
    {
        string[] keys = s.Split('.');

        return string.Join(".", keys.Select(k => k.ToCase(style)));
    }

    /// capitalizes a given string
    /// 'hello' => 'Hello'
    /// 'Hello' => 'Hello'
    /// '' => ''
    private static string Capitalize(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        Span<char> result = stackalloc char[s.Length];
        s.AsSpan().CopyTo(result);

        result[0] = char.ToUpper(result[0]);

        for (int i = 1; i < result.Length; i++)
            result[i] = char.ToLower(result[i]);

        return new string(result);
    }
    
    private static readonly HashSet<char> SymbolSet = [' ', '.', '_', '-', '/', '\\'];

    /// get word list from string input
    /// assume that words are separated by special characters or by camel case
    private static IEnumerable<string> GetWords(this string s)
    {
        bool isAllCaps = true;

        // Проверяем, состоит ли вся строка из заглавных букв
        foreach (char c in s)
        {
            if (char.IsLetter(c) && char.IsLower(c))
            {
                isAllCaps = false;
                break;
            }
        }

        int wordStart = -1;

        for (int i = 0; i < s.Length; i++)
        {
            char currChar = s[i];
            
            char? nextChar = i + 1 < s.Length ? s[i + 1] : null;

            if (SymbolSet.Contains(currChar))
                continue;
            
            if (wordStart == -1) 
                wordStart = i;

            bool isEndOfWord = !nextChar.HasValue ||
                               (!isAllCaps && char.IsUpper(nextChar.Value)) ||
                               SymbolSet.Contains(nextChar.Value);

            if (isEndOfWord)
            { 
                yield return s.Substring(wordStart, i - wordStart + 1);
                wordStart = -1;
            }
        }
    }
}