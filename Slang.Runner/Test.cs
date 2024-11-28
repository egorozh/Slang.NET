using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;

namespace Slang.Runner;

public static class Test
{
    public static RawConfig GetConfig()
    {
        return ConfigRepository.Create(
            inputFileName: "strings",
            @namespace: "Slang.Console",
            className: "Strings",
            baseLocale: "ru-RU");
    }
}