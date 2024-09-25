using System.Globalization;
using Slang.Generator.Config.Entities;

namespace Slang.Generator.Config;

public static class ConfigRepository
{
    public static RawConfig Create(
        string inputFileName,
        string @namespace,
        string className,
        string baseLocale = "en",
        string inputFilePattern = ".i18n.json",
        PluralAuto pluralAuto = PluralAuto.Cardinal,
        string? inputDirectory = null,
        string pluralParameter = "n")
    {
        return new RawConfig(
            Namespace: @namespace,
            ClassName: className,
            BaseLocale: new CultureInfo(baseLocale),
            InputFileName: inputFileName,
            InputDirectory: inputDirectory,
            InputFilePattern: inputFilePattern,
            PluralAuto: pluralAuto,
            PluralParameter: pluralParameter
        );
    }
}