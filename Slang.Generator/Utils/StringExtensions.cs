using System.Text;
using System.Text.RegularExpressions;
using Slang.Generator.Config.Domain.Entities;

namespace Slang.Generator.Utils;

public static partial class StringExtensions
{
    private static readonly Regex UpperAlphaRegex = MyUpperAlphaRegex();

    private static readonly HashSet<char> SymbolSet = [' ', '.', '_', '-', '/', '\\'];

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
                    .Select((word, index) =>
                        index == 0 ? word.ToLower() : word.Capitalize()));
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

        char first = s[0];

        return $"{first.ToString().ToUpper()}{s[1..].ToLower()}";
    }

    /// get word list from string input
    /// assume that words are separated by special characters or by camel case
    private static List<string> GetWords(this string s)
    {
        StringBuilder buffer = new();
        List<string> words = [];

        bool isAllCaps = s.All(c => !char.IsLetter(c) || char.IsUpper(c));

        for (int i = 0; i < s.Length; i++)
        {
            char currChar = s[i];

            char? nextChar = i + 1 == s.Length ? null : s[i + 1];

            if (SymbolSet.Contains(currChar))
                continue;

            buffer.Append(currChar);

            bool isEndOfWord = !nextChar.HasValue ||
                               (!isAllCaps && UpperAlphaRegex.Match(nextChar.Value.ToString()).Success) ||
                               SymbolSet.Contains(nextChar.Value);

            if (isEndOfWord)
            {
                words.Add(buffer.ToString());
                buffer.Clear();
            }
        }

        return words;
    }

    [GeneratedRegex("[A-Z]")]
    private static partial Regex MyUpperAlphaRegex();
}