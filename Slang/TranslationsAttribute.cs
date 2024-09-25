namespace Slang;

[AttributeUsage(AttributeTargets.Class)]
public class TranslationsAttribute
    : Attribute
{
    public string InputFileName { get; init; }

    public string InputFilePattern { get; init; } = ".i18n.json";

    public string BaseLocale { get; init; } = "en";

    public PluralAuto PluralAuto { get; init; } = PluralAuto.Cardinal;

    public string? InputDirectory { get; init; }

    public string PluralParameter { get; init; } = "n";
}