using Slang.Generator.Data;
using Slang.Generator.Domain.Entities;

namespace Slang.Runner;

public static class Test
{
    public static RawConfig GetConfig()
    {
        return ConfigRepository.Create(
            inputFileName: "strings",
            @namespace: "Slang.Showcase",
            className: "Strings",
            baseLocale: "en");
    }

    public static RawConfig GetConfig2()
    {
        return ConfigRepository.Create(
            inputFileName: "feature1",
            @namespace: "Slang.Showcase.MyNamespace",
            className: "Feature1",
            baseLocale: "en");
    }
}