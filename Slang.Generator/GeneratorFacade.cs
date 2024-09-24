using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Generator.Entities;
using Slang.Generator.NodesData;
using Slang.Generator.Translations.Domain;

namespace Slang.Generator;

public static class GeneratorFacade
{
    /// Common step used by custom runner and builder to get the .g.cs content
    public static BuildResult Generate(
        string @namespace,
        string className,
        RawConfig rawConfig,
        string baseName,
        TranslationComposition translationComposition)
    {
        // build translation model
        var translationModelList = NodesDataRepository.GetNodesData(
            rawConfig,
            translationComposition
        );
        
        // generate config
        var config = GetConfig(
            @namespace: @namespace,
            className: className,
            baseName: baseName,
            config: rawConfig
        );

        // generate .g.dart file
        return Generator.Generator.Generate(
            config: config,
            translations: translationModelList
        );
    }

    // test method for benchmark
    public static void GetNodes(
        RawConfig rawConfig,
        TranslationComposition translationComposition)
    {
        var _ = NodesDataRepository.GetNodesData(
            rawConfig,
            translationComposition
        );
    }

    private static GenerateConfig GetConfig(
        string baseName,
        string @namespace,
        string className,
        RawConfig config)
    {
        return new GenerateConfig(
            Namespace: @namespace,
            BaseName: baseName,
            BaseLocale: config.BaseLocale,
            FallbackStrategy: config.FallbackStrategy.ToGenerateFallbackStrategy(),
            className
        );
    }

    private static GenerateFallbackStrategy ToGenerateFallbackStrategy(this FallbackStrategy fallbackStrategy) =>
        fallbackStrategy switch
        {
            FallbackStrategy.none => GenerateFallbackStrategy.None,
            FallbackStrategy.baseLocale => GenerateFallbackStrategy.BaseLocale,
            FallbackStrategy.baseLocaleEmptyString => GenerateFallbackStrategy.BaseLocale,
            _ => throw new ArgumentOutOfRangeException(nameof(fallbackStrategy), fallbackStrategy, null)
        };
}