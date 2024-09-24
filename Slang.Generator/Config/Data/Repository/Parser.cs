using Slang.Generator.Config.Domain.Entities;

namespace Slang.Generator.Config.Data.Repository;

internal static class Parser
{
    public static FallbackStrategy? ToFallbackStrategy(this string s) => s switch
    {
        "none" => FallbackStrategy.None,
        "base_locale" => FallbackStrategy.BaseLocale,
        "base_locale_empty_string" => FallbackStrategy.BaseLocaleEmptyString,
        _ => null
    };

    public static PluralAuto? ToPluralAuto(this string s) => s switch
    {
        "off" => PluralAuto.Off,
        "cardinal" => PluralAuto.Cardinal,
        "ordinal" => PluralAuto.Ordinal,
        _ => null
    };
}