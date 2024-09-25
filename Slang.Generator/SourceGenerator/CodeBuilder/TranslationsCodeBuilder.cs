using Microsoft.CodeAnalysis;
using Slang.Generator.Config;
using Slang.Generator.Config.Entities;
using Slang.Generator.Files;
using Slang.Generator.SourceGenerator.Models;
using Slang.Generator.Translations;

namespace Slang.Generator.SourceGenerator.CodeBuilder;

internal static class TranslationsCodeBuilder
{
    public static async Task Generate(SourceProductionContext context, HierarchyInfo hierarchy, TranslationsParam? info,
        string projectDir)
    {
        string targetDirectory = projectDir;

        string className = hierarchy.MetadataName;

        string namespaceName = hierarchy.Namespace;

        RawConfig config = ConfigRepository.Create(
            className: className,
            @namespace: namespaceName,
            baseLocale: info.BaseLocale,
            inputFileName: info.InputFileName,
            inputDirectory: info.InputDirectory,
            inputFilePattern: info.InputFilePattern
        );

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

        context.AddSource($"{config.ClassName}.g.cs", result.Header);

        foreach ((var locale, string localeTranslations) in result.Translations)
        {
            context.AddSource($"{config.ClassName}_{locale.TwoLetterISOLanguageName}.g.cs", localeTranslations);
        }
    }
}