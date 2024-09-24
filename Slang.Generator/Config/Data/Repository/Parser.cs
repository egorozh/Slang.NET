using Slang.Generator.Config.Domain.Entities;

namespace Slang.Generator.Config.Data.Repository;

internal static class Parser
{
    public static FallbackStrategy? ToFallbackStrategy(this string s) => s switch
    {
        "none" => FallbackStrategy.none,
        "base_locale" => FallbackStrategy.baseLocale,
        "base_locale_empty_string" => FallbackStrategy.baseLocaleEmptyString,
        _ => null
    };

    public static PluralAuto? ToPluralAuto(this string s) => s switch
    {
        "off" => PluralAuto.off,
        "cardinal" => PluralAuto.cardinal,
        "ordinal" => PluralAuto.ordinal,
        _ => null
    };
}