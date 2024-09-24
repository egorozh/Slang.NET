namespace Slang;

public partial class PluralResolvers
{
    /// A very tolerant plural resolver suitable for most languages.
    /// This is used as fallback.
    private static readonly Resolvers DefaultResolver = new(
        Cardinal: (n, zero, one, _, _, _, other) => n switch
        {
            0 => zero ?? other ?? n.ToString(),
            1 => one ?? other ?? n.ToString(),
            _ => other ?? n.ToString()
        },
        Ordinal: (n, _, _, _, _, _, other) => other ?? n.ToString());

    /// Predefined pluralization resolvers
    /// See https://unicode-org.github.io/cldr-staging/charts/latest/supplemental/language_plural_rules.html
    /// Sorted by language alphabetically
    ///
    /// Contribution would be nice! (Only this file needs to be changed)
    private static readonly Dictionary<string, Resolvers> ResolverMap = new()
    {
        // Czech
        {
            "cs", new Resolvers(
                Cardinal: (n, zero, one, _, few, _, other) =>
                {
                    return n switch
                    {
                        0 => zero ?? other!,
                        1 => one ?? other!,
                        >= 2 and <= 4 => few ?? other!,
                        _ => other!
                    };
                },
                Ordinal: (_, _, _, _, _, _, other) => other!)
        },
        // German
        {
            "de", new Resolvers(
                Cardinal: (n, zero, one, _, _, _, other) =>
                {
                    return n switch
                    {
                        0 => zero ?? other!,
                        1 => one ?? other!,
                        _ => other!
                    };
                },
                Ordinal: (_, _, _, _, _, _, other) => other!)
        },
        // English
        {
            "en", new Resolvers(
                Cardinal: (n, zero, one, _, _, _, other) =>
                {
                    return n switch
                    {
                        0 => zero ?? other!,
                        1 => one ?? other!,
                        _ => other!
                    };
                },
                Ordinal: (n, _, one, two, few, _, other) =>
                {
                    if (n % 10 == 1 && n % 100 != 11)
                        return one ?? other!;

                    if (n % 10 == 2 && n % 100 != 12)
                        return two ?? other!;

                    if (n % 10 == 3 && n % 100 != 13)
                        return few ?? other!;

                    return other!;
                }
            )
        },
        // Spanish
        {
            "es", new Resolvers(
                Cardinal: (n, zero, one, _, _, _, other) =>
                {
                    return n switch
                    {
                        0 => zero ?? other!,
                        1 => one ?? other!,
                        _ => other!
                    };
                },
                Ordinal: (_, _, _, _, _, _, other) => other!)
        },
        // French
        {
            "fr", new Resolvers(
                Cardinal: (n, zero, one, _, _, many, other) =>
                {
                    int i = n;
                    int v = i == n ? 0 : n.ToString().Split(".")[1].Length;

                    if (n == 0)
                        return zero ?? one ?? other!;

                    if (i == 1)
                        return one ?? other!;

                    if (i != 0 && i % 1000000 == 0 && v == 0)
                        return many ?? other!;

                    return other!;
                },
                Ordinal: (n, _, _, _, _, many, other) =>
                {
                    if (n == 1)
                        return many ?? other!;

                    return other!;
                }
            )
        },
        // Italian
        {
            "it", new Resolvers(
                Cardinal: (n, zero, one, _, _, _, other) =>
                {
                    int i = n;
                    int v = i == n ? 0 : n.ToString().Split(".")[1].Length;

                    if (n == 0)
                        return zero ?? other!;

                    if (i == 1 && v == 0)
                        return one ?? other!;

                    return other!;
                },
                Ordinal: (n, _, _, _, _, many, other) =>
                {
                    if (n is 8 or 11 or 80 or 800)
                        return many ?? other!;

                    return other!;
                }
            )
        },
        // Russian
        {
            "ru", new Resolvers(
                Cardinal: (n, zero, one, _, few, many, other) =>
                {
                    if (n == 0)
                        return zero ?? other!;

                    int fr10 = n % 10;
                    int fr100 = n % 100;

                    if (fr10 == 1 && fr100 != 11)
                        return one ?? other!;

                    if (Math.Clamp(fr10, 2, 4) == fr10 && Math.Clamp(fr100, 12, 14) != fr100)
                        return few ?? other!;

                    if (fr10 == 0 ||
                        Math.Clamp(fr10, 5, 9) == fr10 ||
                        Math.Clamp(fr100, 11, 14) == fr100)
                    {
                        return many ?? other!;
                    }

                    return other!;
                },
                Ordinal: (_, _, _, _, _, _, other) => other!)
        },
        // Swedish
        {
            "sv", new Resolvers(
                Cardinal: (n, zero, one, _, _, _, other) => n switch
                {
                    0 => zero ?? other!,
                    1 or -1 => one ?? other!,
                    _ => other!
                },
                Ordinal: (n, _, one, _, _, _, other) =>
                {
                    if (n % 10 == 1 && n % 100 != 11)
                        return one ?? other!;

                    if (n % 10 == 2 && n % 100 != 12)
                        return one ?? other!;

                    return other!;
                }
            )
        },
        // Vietnamese
        {
            "vi", new Resolvers(
                Cardinal: (n, zero, _, _, _, _, other) =>
                {
                    if (n == 0)
                        return zero ?? other!;

                    return other!;
                },
                Ordinal: (n, _, one, _, _, _, other) =>
                {
                    if (n == 1)
                        return one ?? other!;

                    return other!;
                }
            )
        }
    };
}