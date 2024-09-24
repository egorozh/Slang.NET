using Slang.Generator;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Files;
using Slang.Generator.Translations.Data;

namespace Slang.Runner;

internal class I18NBuilder(RawConfig config)
{
    public async Task Build()
    {
        // Glob findAssetsPattern = config.inputDirectory != null
        //     ? Glob($"**{config.inputDirectory}/**{config.inputFilePattern}")
        //     : Glob($"**{config.inputFilePattern}");

        // STEP 1: determine base name and output file name / path

        // var assets = await buildStep.findAssets(findAssetsPattern).toList();
        //
        // var files = assets.Select((f) =>
        // {
        //     return new PlainTranslationFile(
        //         path: f.path,
        //         read: () => buildStep.readAsString(f)
        //     );
        // }).toList();

        //todo: getting input info from NET SOURCE GENERATOR
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        string outputFileName = "Strings.g.cs";
        string outputDirectory = "i18n";

        string[] paths = ["strings_en.i18n.json", "strings_ru.i18n.json"];

        var files = paths
            .Select(p => Path.Combine(targetDirectory, "i18n", p))
            .Select(f => new FileInfo(f))
            .ToList();

        var fileCollection = FilesRepository.GetFileCollection(
            config: config,
            allFiles: files
        );

        string outputFilePath = DetermineOutputPath(fileCollection, outputFileName, outputDirectory);

        // STEP 2: scan translations
        var translationMap = await TranslationsRepository.Build(
            fileCollection: fileCollection,
            verbose: false
        );

        //todo: get namespace from SG
        const string @namespace = "Slang.Showcase";
        const string className = "Strings";

        // STEP 3: generate .g.dart content
        var result = GeneratorFacade.Generate(
            @namespace,
            className,
            rawConfig: config,
            baseName: PathUtils.GetFileNameNoExtension(outputFileName),
            translationComposition: translationMap
        );

        // STEP 4: write output to hard drive
        FileUtils.createMissingFolders(filePath: outputFilePath);


        // multiple files
        FileUtils.writeFile(
            path: BuildResultPaths.MainPath(outputFilePath),
            content: result.Header
        );

        foreach (var entry in result.Translations)
        {
            var locale = entry.Key;
            string localeTranslations = entry.Value;
            FileUtils.writeFile(
                path: BuildResultPaths.LocalePath(
                    outputPath: outputFilePath,
                    locale: locale
                ),
                content: localeTranslations
            );
        }
    }

    private static string DetermineOutputPath(SlangFileCollection fileCollection,
        string outputFileName, string? outputDirectory)
    {
        if (outputDirectory != null)
        {
            // output directory specified, use this path instead
            return $"{outputDirectory}/{outputFileName}";
        }

        // use the directory of the first (random) translation file
        string tempPath = fileCollection.Files.First().Path;

        // By default, generate to the same directory as the translation file
        return PathUtils.ReplaceFileName(
            path: tempPath,
            newFileName: outputFileName,
            pathSeparator: "/"
        );
    }
}