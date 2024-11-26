using NodeHelpers = Slang.Generator.Core.Nodes.Utils.NodeHelpers;

namespace Slang.Generator.Core.Nodes.Nodes;

internal record Placeholder(
    string? Type,
    string? Format
);

internal record ExtendData(
    string? Description,
    IReadOnlyDictionary<string, Placeholder>? Placeholders = null);

/// <summary>
/// The super class of every node
/// </summary>
internal abstract record Node(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData);

/// <summary>
/// The super class for list and object nodes
/// </summary>
/// <param name="GenericType">The generic type of the container, i.e. <see cref="Dictionary{String, T}"/> or <see cref="List{T}"/></param>
internal abstract record IterableNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData,
    string GenericType)
    : Node(Path, Modifiers, ExtendData);

internal record ListNode(
    string Path,
    ExtendData? ExtendData,
    IReadOnlyDictionary<string, string> Modifiers,
    List<Node> Entries)
    : IterableNode(Path, Modifiers, ExtendData, NodeHelpers.DetermineGenericType(Entries));

internal record ObjectNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData,
    Dictionary<string, Node> Entries,
    bool IsMap)
    : IterableNode(Path, Modifiers, ExtendData, NodeHelpers.DetermineGenericType(Entries.Values));

internal interface ILeafNode;