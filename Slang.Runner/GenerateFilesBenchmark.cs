using BenchmarkDotNet.Attributes;
using Slang.Generator.Data;
using Slang.Generator.Domain;
using Slang.Generator.Domain.Entities;

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

        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        string sourceFilesDirectory = _config.InputDirectory != null
            ? Path.Combine(targetDirectory, _config.InputDirectory)
            : targetDirectory;

        var paths = Directory.GetFiles(sourceFilesDirectory, _config.InputFilePattern)
            .Where(file => Path.GetFileName(file).StartsWith(_config.InputFileName));

        var files = paths
            .Select(f => new FileInfo(f))
            .ToList();

        var fileCollection = FilesRepository.GetFileCollection(
            config: _config,
            allFiles: files
        );

        _translationMap = await TranslationsRepository.Build(
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
            translationComposition: _translationMap
        );
    }
}