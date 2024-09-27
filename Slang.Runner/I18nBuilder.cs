using Slang.Generator.Data;
using Slang.Generator.Domain;
using Slang.Generator.Domain.Entities;


namespace Slang.Runner;

internal class I18NBuilder(RawConfig config)
{
    public async Task Build()
    {
        //todo: getting input info from NET SOURCE GENERATOR
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        string sourceFilesDirectory = config.InputDirectory != null
            ? Path.Combine(targetDirectory, config.InputDirectory)
            : targetDirectory;

        var paths = Directory.GetFiles(sourceFilesDirectory, config.InputFilePattern)
            .Where(file => Path.GetFileName(file).StartsWith(config.InputFileName));

        var files = paths
            .Select(f => new FileInfo(f))
            .ToList();

        var fileCollection = FilesRepository.GetFileCollection(
            config: config,
            allFiles: files
        );

        // STEP 2: scan translations
        var translationMap = await TranslationsRepository.Build(fileCollection: fileCollection);

        // STEP 3: generate .g.dart content
        var result = GeneratorFacade.Generate(
            rawConfig: config,
            translationComposition: translationMap
        );

        await File.WriteAllTextAsync(
            Path.Combine(sourceFilesDirectory, $"{config.ClassName}.g.cs"),
            result.Header
        );

        foreach ((var locale, string localeTranslations) in result.Translations)
        {
            await File.WriteAllTextAsync(
                Path.Combine(sourceFilesDirectory, $"{config.ClassName}_{locale.TwoLetterISOLanguageName}.g.cs"),
                localeTranslations
            );
        }
    }
}