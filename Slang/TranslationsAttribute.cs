namespace Slang;

[AttributeUsage(AttributeTargets.Class)]
public class TranslationsAttribute(
    string inputFileName,
    string baseLocale = "en",
    string inputFilePattern = ".i18n.json",
    PluralAuto pluralAuto = PluralAuto.Cardinal,
    string? inputDirectory = null,
    string pluralParameter = "n")
    : Attribute
{
    public string InputFileName { get; } = inputFileName;
    
    public string InputFilePattern { get; } = inputFilePattern;
    
    public string BaseLocale { get; } = baseLocale;
    
    public PluralAuto PluralAuto { get; } = pluralAuto;
    
    public string? InputDirectory { get; } = inputDirectory;
    
    public string PluralParameter { get; } = pluralParameter;
}