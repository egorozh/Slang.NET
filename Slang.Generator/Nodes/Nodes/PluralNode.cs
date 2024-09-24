using Slang.Generator.Nodes.Domain;

namespace Slang.Generator.Nodes.Nodes;

internal enum PluralType
{
    Cardinal,
    Ordinal
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
    Dictionary<string, string> Modifiers,
    string? Comment,
    PluralType PluralType,
    Dictionary<Quantity, StringTextNode> Quantities,
    string ParamName,
    string ParamType) : Node(Path, Modifiers, Comment), ILeafNode;