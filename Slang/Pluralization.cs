namespace Slang;

/// Selects the correct string depending on [n]
public delegate string PluralResolver(
    int n,
    string? zero = null,
    string? one = null,
    string? two = null,
    string? few = null,
    string? many = null,
    string? other = null
);

/// Default plural resolvers
public static partial class PluralResolvers
{
    private record Resolvers(
        PluralResolver Cardinal,
        PluralResolver Ordinal
    );

    public static PluralResolver Cardinal(string language) => GetResolvers(language).Cardinal;

    public static PluralResolver Ordinal(string language) => GetResolvers(language).Ordinal;

    private static Resolvers GetResolvers(string language)
    {
        if (!ResolverMap.TryGetValue(language, out var resolvers))
        {
            Console.WriteLine(
                $"Resolver for <lang = {language}> not specified! Please configure it via LocaleSettings.setPluralResolver. A fallback is used now.");

            return DefaultResolver;
        }

        return resolvers;
    }
}