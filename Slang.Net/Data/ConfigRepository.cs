using System.Globalization;
using Slang.Generator.Core.Entities;

namespace Slang.Generator.Core.Data;

public static class ConfigRepository
{
    public static RawConfig Create(
        string inputFileName,
        string @namespace,
        string className,
        string baseLocale = "en",
        PluralAutoEntity pluralAutoEntity = PluralAutoEntity.Cardinal,
        string rootPropertyName = "Root",
        string pluralParameter = "n",
        string startCharacter = "{",
        string endCharacter = "}")
    {
        return new RawConfig(
            Namespace: @namespace,
            ClassName: className,
            BaseLocale: new CultureInfo(baseLocale),
            InputFileName: inputFileName,
            PluralAutoEntity: pluralAutoEntity,
            PluralParameter: pluralParameter,
            RootPropertyName: rootPropertyName,
            StartCharacter: startCharacter,
            EndCharacter: endCharacter
        );
    }
}