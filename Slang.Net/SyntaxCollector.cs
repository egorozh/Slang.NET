namespace Slang;

internal class SyntaxCollector : ISyntaxReceiver
{
    public List<TypeDeclarationSyntax> CandidateTypes { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (IsCandidateType(syntaxNode))
        {
            CandidateTypes.Add((TypeDeclarationSyntax)syntaxNode);
        }
    }
    
    public static bool IsCandidateType(SyntaxNode syntax)
    {
        if (syntax is not TypeDeclarationSyntax typeDeclarationSyntax)
        {
            return false;
        }

        foreach (var attributeList in typeDeclarationSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (IsKnownAttribute(attribute))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsKnownAttribute(SyntaxNode syntaxNode)
    {
        if (syntaxNode is AttributeSyntax
            {
                Name: SimpleNameSyntax
                {
                    Identifier: { } identifier
                },
                Parent: AttributeListSyntax
                {
                    Parent: TypeDeclarationSyntax
                }
            })
        {
            switch (identifier.Text)
            {
                case KnownTypes.TranslationsAttributeShortName:
                case KnownTypes.TranslationsAttributeTypeName:
                    return true;
            }
        }

        return false;
    }
}