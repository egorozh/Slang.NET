using System.Globalization;
using Slang.Generator.Domain.Entities;

namespace Slang.Generator.Data;

public static class ConfigRepository
{
    public static RawConfig Create(
        string inputFileName,
        string @namespace,
        string className,
        string baseLocale = "en",
        PluralAuto pluralAuto = PluralAuto.Cardinal,
        string pluralParameter = "n")
    {
        return new RawConfig(
            Namespace: @namespace,
            ClassName: className,
            BaseLocale: new CultureInfo(baseLocale),
            InputFileName: inputFileName,
            PluralAuto: pluralAuto,
            PluralParameter: pluralParameter
        );
    }
}