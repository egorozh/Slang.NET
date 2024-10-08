using Microsoft.CodeAnalysis;
using Slang.Generator.SourceGenerator.Extensions;
using static Microsoft.CodeAnalysis.SymbolDisplayTypeQualificationStyle;


namespace Slang.Generator.SourceGenerator.Models;


internal sealed record HierarchyInfo(string FilenameHint, string MetadataName, string Namespace)
{
    public static HierarchyInfo From(INamedTypeSymbol typeSymbol) => new(
        typeSymbol.GetFullyQualifiedMetadataName(),
        typeSymbol.MetadataName,
        typeSymbol.ContainingNamespace.ToDisplayString(new(typeQualificationStyle: NameAndContainingTypesAndNamespaces)));
}