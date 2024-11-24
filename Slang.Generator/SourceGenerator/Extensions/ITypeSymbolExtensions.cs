using Microsoft.CodeAnalysis;
using Slang.Generator.SourceGenerator.Helpers;

namespace Slang.Generator.SourceGenerator.Extensions;

internal static class TypeSymbolExtensions
{
    public static string GetFullyQualifiedMetadataName(this ITypeSymbol symbol)
    {
        using var builder = ImmutableArrayBuilder<char>.Rent();

        symbol.AppendFullyQualifiedMetadataName(in builder);

        return builder.ToString();
    }
    
    private static void AppendFullyQualifiedMetadataName(this ITypeSymbol symbol,
        in ImmutableArrayBuilder<char> builder)
    {
        BuildFrom(symbol, in builder);
        return;

        static void BuildFrom(ISymbol? symbol, in ImmutableArrayBuilder<char> builder)
        {
            switch (symbol)
            {
                // Namespaces that are nested also append a leading '.'
                case INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: false }:
                    BuildFrom(symbol.ContainingNamespace, in builder);
                    builder.Add('.');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Other namespaces (ie. the one right before global) skip the leading '.'
                case INamespaceSymbol { IsGlobalNamespace: false }:
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol { IsGlobalNamespace: true } }:
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Types with no namespace just have their metadata name directly written

                // Types with a containing non-global namespace also append a leading '.'
                case ITypeSymbol { ContainingSymbol: INamespaceSymbol namespaceSymbol }:
                    BuildFrom(namespaceSymbol, in builder);
                    builder.Add('.');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;

                // Nested types append a leading '+'
                case ITypeSymbol { ContainingSymbol: ITypeSymbol typeSymbol }:
                    BuildFrom(typeSymbol, in builder);
                    builder.Add('+');
                    builder.AddRange(symbol.MetadataName.AsSpan());
                    break;
            }
        }
    }
}