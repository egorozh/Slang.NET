namespace Slang.Generator.Core.Nodes.Nodes;

public record StringTextNode : Node, ILeafNode
{
    public StringTextNode(string Path,
        IReadOnlyDictionary<string, string> Modifiers,
        ExtendData? ExtendData,
        HashSet<string> Params,
        Dictionary<string, string> ParamTypeMap,
        HashSet<string> Links,
        string Content,
        string ParsedContent) : base(Path, Modifiers, ExtendData)
    {
        this.Params = Params;
        this.ParamTypeMap = ParamTypeMap;
        this.Links = Links;
        this.Content = Content;
        this.ParsedContent = ParsedContent;
    }

    /// <summary>
    /// Set of parameters.
    /// Hello {name}, I am {age} years old -> {'name', 'age'}
    /// </summary>
    public HashSet<string> Params { get; }

    /// <summary>
    /// Plural and context parameters need to have a special parameter type (e.g. num)
    /// In a normal case, this parameter and its type will be added at generate stage
    /// For special cases, i.e. a translation is linked to a plural translation,
    /// the type must be specified and cannot be [Object].
    /// </summary>
    public Dictionary<string, string> ParamTypeMap { get; set; }

    /// <summary>
    /// Set of paths to [TextNode]s
    /// Will be used for 2nd round, determining the final set of parameters
    /// </summary>
    public HashSet<string> Links { get; }
    
    /// <summary>
    /// Content of the text node, normalized.
    /// Will be written to .g.cs as is.
    /// </summary>
    public string Content { get; set; }

    public string ParsedContent { get; }
}