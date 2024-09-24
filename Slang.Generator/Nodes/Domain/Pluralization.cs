namespace Slang.Generator.Nodes.Domain;

public enum Quantity
{
    zero,
    one,
    two,
    few,
    many,
    other
}

public static class Pluralization
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