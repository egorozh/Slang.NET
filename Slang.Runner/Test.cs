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
            baseLocale: "en");
    }

    public static RawConfig GetConfig2()
    {
        return ConfigRepository.Create(
            inputFileName: "feature1",
            @namespace: "Slang.Console.MyNamespace",
            className: "Feature1",
            baseLocale: "en");
    }
}