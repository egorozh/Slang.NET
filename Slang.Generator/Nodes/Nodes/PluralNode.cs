namespace Slang.Generator.Nodes.Nodes;

internal enum PluralType
{
    Cardinal,
    Ordinal
}

internal enum Quantity
{
    zero,
    one,
    two,
    few,
    many,
    other
}

/// <param name="Path"></param>
/// <param name="Modifiers"></param>
/// <param name="Comment"></param>
/// <param name="PluralType"></param>
/// <param name="Quantities"></param>
/// <param name="ParamName">name of the plural parameter</param>
/// <param name="ParamType">type of the plural parameter defaults to num</param>
internal record PluralNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    string? Comment,
    PluralType PluralType,
    Dictionary<Quantity, StringTextNode> Quantities,
    string ParamName,
    string ParamType) : Node(Path, Modifiers, Comment), ILeafNode;

internal static class Pluralization
{
    public static Quantity[] AllQuantities =>
    [
        Quantity.zero,
        Quantity.one,
        Quantity.two,
        Quantity.few,
        Quantity.many,
        Quantity.other
    ];
}

internal static class QuantityExtensions
{
    public static string ParamName(this Quantity quantity) => quantity switch
    {
        Quantity.zero => "zero",
        Quantity.one => "one",
        Quantity.two => "two",
        Quantity.few => "few",
        Quantity.many => "many",
        Quantity.other => "other",
        _ => throw new ArgumentOutOfRangeException(nameof(quantity), quantity, null)
    };
}

internal static class QuantityParser
{
    public static Quantity? ToQuantity(this string s) => s.ToLower() switch
    {
        "zero" => Quantity.zero,
        "one" => Quantity.one,
        "two" => Quantity.two,
        "few" => Quantity.few,
        "many" => Quantity.many,
        "other" => Quantity.other,
        _ => null
    };
}