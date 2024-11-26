using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Slang.Generator.SourceGenerator.Extensions;


internal static class TypeDeclarationSyntaxExtensions
{
    public static bool HasOrPotentiallyHasAttributes(this TypeDeclarationSyntax typeDeclaration)
    {
        if (typeDeclaration.AttributeLists.Count > 0)
        {
            return true;
        }
        
        
        foreach (SyntaxToken modifier in typeDeclaration.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PartialKeyword))
            {
                return true;
            }
        }

        return false;
    }
}
