namespace Slang.Generator.Domain.Nodes.Nodes;

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

/// <param name="ParamName">name of the plural parameter</param>
/// <param name="ParamType">type of the plural parameter defaults to num</param>
internal record PluralNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData,
    PluralType PluralType,
    Dictionary<Quantity, StringTextNode> Quantities,
    string ParamName,
    string ParamType) : Node(Path, Modifiers, ExtendData), ILeafNode;

internal static class Pluralization
{
    public static HashSet<string> AllQuantities =>
    [
        "zero",
        "one",
        "two",
        "few",
        "many",
        "other"
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