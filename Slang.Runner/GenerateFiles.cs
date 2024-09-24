using BenchmarkDotNet.Attributes;
using Slang.Generator;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Files;
using Slang.Generator.Translations;

namespace Slang.Runner;

[MemoryDiagnoser]
public class GenerateFiles
{
    private readonly string _namespace;
    private readonly string _className;
    private readonly RawConfig _config;
    private readonly string _outputFileName;
    private TranslationComposition _translationMap;
    private SlangFileCollection _fileCollection;

    public GenerateFiles()
    {
        const string @namespace = "Slang.Showcase";
        const string className = "Strings";

        _config = Test.GetConfig([]);
        _namespace = @namespace;
        _className = className;
        _outputFileName = "Strings.g.cs";
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        string[] paths = ["strings_en.i18n.json", "strings_ru.i18n.json"];
        var files = paths
            .Select(p => Path.Combine(targetDirectory, "i18n", p))
            .Select(f => new FileInfo(f))
            .ToList();

        _fileCollection = FilesRepository.GetFileCollection(
            config: _config,
            allFiles: files
        );

        _translationMap = await TranslationsRepository.Build(
            fileCollection: _fileCollection
        );
    }

    [Benchmark]
    public async Task GetTranslationMap()
    {
        _translationMap = await TranslationsRepository.Build(
            fileCollection: _fileCollection
        );
    }

    [Benchmark]
    public void FullGeneration()
    {
        var result = GeneratorFacade.Generate(
            _namespace,
            _className,
            rawConfig: _config,
            baseName: PathUtils.GetFileNameNoExtension(_outputFileName),
            translationComposition: _translationMap
        );
    }

    [Benchmark]
    public void GetNodes()
    {
        GeneratorFacade.GetNodes(_config, _translationMap);
    }
}