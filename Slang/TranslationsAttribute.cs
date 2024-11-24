namespace Slang;

[AttributeUsage(AttributeTargets.Class)]
public class TranslationsAttribute
    : Attribute
{
    public string? InputFileName { get; init; }

    public string? RootPropertyName { get; init; }

    public PluralAuto PluralAuto { get; init; }

    public string? PluralParameter { get; init; }
}