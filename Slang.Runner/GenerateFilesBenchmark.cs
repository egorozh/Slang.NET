using BenchmarkDotNet.Attributes;
using Slang.Generator.Core;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;

namespace Slang.Runner;

[MemoryDiagnoser]
public class GenerateFilesBenchmark
{
    private RawConfig _config = null!;
    private TranslationComposition _translationMap = null!;
    private GeneratorFacade.BenchmarkGeneratorData _benchData = null!;
    
    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _config = Test.GetConfig();

        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Examples/Slang.Console";

        string sourceFilesDirectory = Path.Combine(targetDirectory, "i18n");

        var paths = Directory.GetFiles(sourceFilesDirectory, "*.i18n.json")
            .Where(file => Path.GetFileName(file).StartsWith(_config.InputFileName));

        var files = paths
            .Select(f => new FileInfo(f))
            .ToList();

        var fileCollection = FilesRepository.GetFileCollection(
            _config.BaseLocale,
            allFiles: files
        );

        _translationMap = await TranslationsRepository.Build(
            _config.BaseLocale,
            fileCollection: fileCollection
        );

        _benchData =
            new GeneratorFacade.BenchmarkGeneratorData(_config, _translationMap);
    }

    [Benchmark]
    public void GetNodes()
    {
        GeneratorFacade.GetNodes(_config, _translationMap);
    }

    [Benchmark]
    public void OnlyGeneration()
    {
        var _ = GeneratorFacade.GenerateOnly(_benchData);
    }

    [Benchmark]
    public void FullGeneration()
    {
        var _ = GeneratorFacade.Generate(
            rawConfig: _config,
            translationComposition: _translationMap,
            DateTime.Now
        );
    }
}