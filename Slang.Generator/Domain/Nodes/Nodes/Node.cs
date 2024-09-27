using NodeHelpers = Slang.Generator.Domain.Nodes.Utils.NodeHelpers;

namespace Slang.Generator.Domain.Nodes.Nodes;

/// <summary>
/// The super class of every node
/// </summary>
/// <param name="Path"></param>
/// <param name="Modifiers"></param>
/// <param name="Comment"></param>
internal abstract record Node(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    string? Comment);

/// <summary>
/// The super class for list and object nodes
/// </summary>
/// <param name="Path"></param>
/// <param name="Modifiers"></param>
/// <param name="Comment"></param>
/// <param name="GenericType">The generic type of the container, i.e. Map<String, T> or List<T></param>
internal abstract record IterableNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    string? Comment,
    string GenericType)
    : Node(Path, Modifiers, Comment);

internal record ListNode(
    string Path,
    string? Comment,
    IReadOnlyDictionary<string, string> Modifiers,
    List<Node> Entries)
    : IterableNode(Path, Modifiers, Comment, NodeHelpers.DetermineGenericType(Entries));

internal record ObjectNode(
    string Path,
    IReadOnlyDictionary<string, string> Modifiers,
    string? Comment,
    Dictionary<string, Node> Entries,
    bool IsMap)
    : IterableNode(Path, Modifiers, Comment, NodeHelpers.DetermineGenericType(Entries.Values));

internal interface ILeafNode;