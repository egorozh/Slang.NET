using Slang.Generator.Core;
using Slang.Generator.Core.Data;
using Slang.Generator.Core.Entities;

namespace Slang.Runner;

internal class Generator(RawConfig config, string filesDirectory)
{
    public async Task Generate()
    {
        var paths = Directory.GetFiles(filesDirectory, "*.i18n.json")
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
            Path.Combine(filesDirectory, $"{config.ClassName}.g.cs"),
            result.Header
        );

        foreach ((var locale, string localeTranslations) in result.Translations)
        {
            await File.WriteAllTextAsync(
                Path.Combine(filesDirectory, $"{config.ClassName}_{locale}.g.cs"),
                localeTranslations
            );
        }
    }
}