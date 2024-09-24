namespace Slang;

[AttributeUsage(AttributeTargets.Class)]
public class TranslationsAttribute(
    string baseLocale = "en",
    string inputFilePattern = ".i18n.json",
    FallbackStrategy fallbackStrategy = FallbackStrategy.None,
    PluralAuto pluralAuto = PluralAuto.Cardinal,
    string? inputDirectory = null,
    string pluralParameter = "n")
    : Attribute
{
    public string InputFilePattern { get; } = inputFilePattern;
    
    public string BaseLocale { get; } = baseLocale;
    
    public FallbackStrategy FallbackStrategy { get; } = fallbackStrategy;
    
    public PluralAuto PluralAuto { get; } = pluralAuto;
    
    public string? InputDirectory { get; } = inputDirectory;
    
    public string PluralParameter { get; } = pluralParameter;
}