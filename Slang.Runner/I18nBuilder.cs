using Slang.Generator.Data;
using Slang.Generator.Domain;
using Slang.Generator.Domain.Entities;


namespace Slang.Runner;

internal class I18NBuilder(RawConfig config)
{
    public async Task Build()
    {
        //todo: getting input info from NET SOURCE GENERATOR
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Console";

        string sourceFilesDirectory = Path.Combine(targetDirectory, "i18n");

        var paths = Directory.GetFiles(sourceFilesDirectory, "*.i18n.json")
            .Where(file => Path.GetFileName(file).StartsWith(config.InputFileName));

        var files = paths
            .Select(f => new FileInfo(f))
            .ToList();

        var fileCollection = FilesRepository.GetFileCollection(
            config.BaseLocale,
            allFiles: files
        );

        // STEP 2: scan translations
        var translationMap = await TranslationsRepository.Build(config, fileCollection: fileCollection);

        // STEP 3: generate .g.dart content
        var result = GeneratorFacade.Generate(
            rawConfig: config,
            translationComposition: translationMap,
            DateTime.Now
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