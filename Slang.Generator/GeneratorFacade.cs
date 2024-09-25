using Slang.Generator.Config.Entities;
using Slang.Generator.Generator.Entities;
using Slang.Generator.NodesData;
using Slang.Generator.Translations;

namespace Slang.Generator;

public static class GeneratorFacade
{
    /// Common step used by custom runner and builder to get the .g.cs content
    public static BuildResult Generate(
        RawConfig rawConfig,
        TranslationComposition translationComposition)
    {
        // build translation model
        var translationModelList = NodesDataRepository.GetNodesData(
            rawConfig,
            translationComposition
        );

        // generate config
        var config = GetConfig(
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

    // test method for benchmark
    public static BuildResult GenerateOnly(BenchmarkGeneratorData input)
    {
        return Generator.Generator.Generate(
            config: input.Config,
            translations: input.TranslationModelList
        );
    }

    private static GenerateConfig GetConfig(
        RawConfig config)
    {
        return new GenerateConfig(
            Namespace: config.Namespace,
            ClassName: config.ClassName,
            BaseLocale: config.BaseLocale
        );
    }

    // test data for benchmark
    public class BenchmarkGeneratorData(
        RawConfig rawConfig,
        TranslationComposition translationMap)
    {
        internal readonly GenerateConfig Config = GetConfig(
            config: rawConfig
        );

        internal readonly List<I18NData> TranslationModelList = NodesDataRepository.GetNodesData(
            rawConfig,
            translationMap
        );
    }
}