namespace Slang;

[AttributeUsage(AttributeTargets.Class)]
public class TranslationsAttribute
    : Attribute
{
    public string InputFileName { get; init; }
    
    public string BaseLocale { get; init; } = "en";

    public PluralAuto PluralAuto { get; init; } = PluralAuto.Cardinal;
    
    public string PluralParameter { get; init; } = "n";
}