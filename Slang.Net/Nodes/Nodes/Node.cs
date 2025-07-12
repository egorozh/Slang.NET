using NodeHelpers = Slang.Generator.Core.Nodes.Utils.NodeHelpers;

namespace Slang.Generator.Core.Nodes.Nodes;

public record Placeholder(
    string? Type,
    string? Format
);

public record ExtendData(
    string? Description,
    IReadOnlyDictionary<string, Placeholder>? Placeholders = null);

/// <summary>
/// The super class of every node
/// </summary>
public abstract record Node(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData);

/// <summary>
/// The super class for list and object nodes
/// </summary>
/// <param name="GenericType">The generic type of the container, i.e. <see cref="Dictionary{String, T}"/> or <see cref="List{T}"/></param>
public abstract record IterableNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData,
    string GenericType)
    : Node(Path, Modifiers, ExtendData);

public record ListNode(
    string Path,
    ExtendData? ExtendData,
    IReadOnlyDictionary<string, string> Modifiers,
    List<Node> Entries)
    : IterableNode(Path, Modifiers, ExtendData, NodeHelpers.DetermineGenericType(Entries));

public record ObjectNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    ExtendData? ExtendData,
    Dictionary<string, Node> Entries,
    bool IsMap)
    : IterableNode(Path, Modifiers, ExtendData, NodeHelpers.DetermineGenericType(Entries.Values));

internal interface ILeafNode;