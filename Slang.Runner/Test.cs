using Slang.Generator.Data;
using Slang.Generator.Domain.Entities;

namespace Slang.Runner;

public static class Test
{
    public static RawConfig GetConfig()
    {
        var config = ConfigRepository.Create(
            className: "Strings",
            @namespace: "Slang.Showcase",
            baseLocale: "en",
            inputFileName: "strings",
            inputDirectory: "i18n",
            inputFilePattern: "*.i18n.json"
        );

        return config;
    }

    public static RawConfig GetConfig2()
    {
        return ConfigRepository.Create(
            className: "Feature1",
            @namespace: "Slang.Showcase.MyNamespace",
            baseLocale: "en",
            inputFileName: "feature1",
            inputDirectory: "i18n",
            inputFilePattern: "*.i18n.json"
        );
    }
}