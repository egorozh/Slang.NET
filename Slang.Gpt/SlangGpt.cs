using System.Globalization;
using Slang.Generator.Config.Domain;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Files;
using Slang.Generator.Translations;
using Slang.Gpt.Models;

namespace Slang.Gpt;

public static class SlangGpt
{
    public static async Task Execute(
        GptConfig gptConfig,
        string apiKey,
        List<CultureInfo>? targetLocales = null,
        string? outDir = null,
        bool debug = false,
        bool full = false)
    {
        var fileCollection = await ReadFromFileSystem(verbose: false);

        if (outDir == null)
        {
            outDir = fileCollection.Config.InputDirectory;

            if (outDir == null)
                throw new Exception("input_directory or --outdir=<path> must be specified.");
        }

        Console.WriteLine(
            $"GPT config: {gptConfig.Model.Id} / {gptConfig.MaxInputLength} max input length / {gptConfig.Temperature?.ToString() ?? "default"} temperature"
        );

        if (gptConfig.Excludes.Count > 0)
        {
            Console.WriteLine(
                $"Excludes: {string.Join(", ", gptConfig.Excludes.Select(e => e.EnglishName))}");
        }

        int promptCount = 0;
        int inputTokens = 0;
        int outputTokens = 0;

        foreach (var file in fileCollection.Files)
        {
            if (!Equals(file.Locale, fileCollection.Config.BaseLocale))
            {
                // Only use base locale as source
                continue;
            }

            string raw = await file.Read();

            Dictionary<string, object> originalTranslations;
            try
            {
                originalTranslations =
                    TranslationsDecoder.DecodeWithFileType(raw);
            }
            catch (Exception e)
            {
                throw new Exception($"File: ${file.Path}\n{e}");
            }

            if (targetLocales == null)
            {
                // translate to existing locales
                foreach (var destFile in fileCollection.Files)
                {
                    if (gptConfig.Excludes.Contains(destFile.Locale))
                    {
                        // skip excluded locales
                        continue;
                    }
                    
                    if (Equals(destFile.Locale, file.Locale))
                    {
                        // skip same locale
                        continue;
                    }

                    var metrics = await SlangGptTranslator.Translate(
                        fileCollection: fileCollection,
                        gptConfig: gptConfig,
                        targetLocale: destFile.Locale,
                        outDir: outDir,
                        full: full,
                        debug: debug,
                        file: file,
                        originalTranslations: originalTranslations,
                        apiKey: apiKey,
                        promptCount: promptCount
                    );

                    promptCount = metrics.EndPromptCount;
                    inputTokens += metrics.InputTokens;
                    outputTokens += metrics.OutputTokens;
                }
            }
            else
            {
                // translate to specified locales (they may not exist yet)
                foreach (var targetLocale in targetLocales)
                {
                    var metrics = await SlangGptTranslator.Translate(
                        fileCollection: fileCollection,
                        gptConfig: gptConfig,
                        targetLocale: targetLocale,
                        outDir: outDir,
                        full: full,
                        debug: debug,
                        file: file,
                        originalTranslations: originalTranslations,
                        apiKey: apiKey,
                        promptCount: promptCount
                    );

                    promptCount = metrics.EndPromptCount;
                    inputTokens += metrics.InputTokens;
                    outputTokens += metrics.OutputTokens;
                }
            }
        }

        Console.WriteLine("");
        Console.WriteLine("Summary:");
        Console.WriteLine($" -> Total requests: {promptCount}");
        Console.WriteLine($" -> Total input tokens: {inputTokens}");
        Console.WriteLine($" -> Total output tokens: {outputTokens}");
        Console.WriteLine(
            $" -> Total cost: ${inputTokens * gptConfig.Model.CostPerInputToken + outputTokens * gptConfig.Model.CostPerOutputToken} ($inputTokens x ${gptConfig.Model.CostPerInputToken} + $outputTokens x ${gptConfig.Model.CostPerOutputToken})");
    }

    private static async Task<SlangFileCollection> ReadFromFileSystem(bool verbose)
    {
        string currentDirectory = Directory.GetCurrentDirectory().Replace("\\", "/");

        if (!currentDirectory.EndsWith('/'))
        {
            currentDirectory += "/";
        }

        DirectoryInfo currentDirectoryInfo = new(currentDirectory);

        // config file must be in top-level directory
        var topLevelFiles = currentDirectoryInfo.GetFiles();

        var config = await ReadConfigFromFileSystem(
            files: topLevelFiles,
            verbose: verbose
        );

        List<FileInfo> files;

        if (config.InputDirectory != null)
        {
            files = new DirectoryInfo(config.InputDirectory!).GetFiles("*", SearchOption.AllDirectories).ToList();
        }
        else
        {
            files = _getFilesBreadthFirst(
                rootDirectory: currentDirectoryInfo,
                ignoreTopLevelDirectories:
                [
                    "build",
                    "ios",
                    "android",
                    "web",
                    "macos",
                    "linux",
                    "windows",
                    "test"
                ],
                ignoreDirectories:
                [
                    ".fvm",
                    ".flutter.git",
                    ".dart_tool",
                    ".symlinks"
                ]
            );
        }

        return FilesRepository.GetFileCollection(
            config: config,
            allFiles: files.Where(f => f.FullName.EndsWith(config.InputFilePattern))
                // .Select(f =>
                // {
                //     return new PlainTranslationFile(
                //         path: f.FullName.Replace("\\", "/").Replace(currentDirectory, ""),
                //         read: () => File.ReadAllTextAsync(f.FullName)
                //     );
                // })
                .ToList()
        );
    }

    private static async Task<RawConfig> ReadConfigFromFileSystem(FileInfo[] files, bool verbose)
    {
        RawConfig? config = null;

        foreach (var file in files)
        {
            if (file.Name != "slang.yaml")
                continue;

            string content = await File.ReadAllTextAsync(file.FullName);

            config = ConfigUseCases.GetRawConfigOrDefault(content);

            if (config == null)
                continue;

            if (verbose)
                Console.WriteLine("Found slang.yaml!");

            break;
        }

        bool useDefaultConfig = config == null;

        if (config == null)
        {
            config = ConfigUseCases.CreateDefaultConfig();

            if (verbose)
                Console.WriteLine("No slang.yaml, using default settings.");
        }

        // show build config
        if (verbose && !useDefaultConfig)
        {
            Console.WriteLine("");
            PrintConfig(config);
            Console.WriteLine("");
        }

        return config;
    }

    /// Returns all files in directory.
    /// Also scans sub directories using the breadth-first approach.
    static List<FileInfo> _getFilesBreadthFirst(
        DirectoryInfo rootDirectory,
        HashSet<string> ignoreTopLevelDirectories,
        HashSet<string> ignoreDirectories
    )
    {
        List<FileInfo> result = [];
        var queue = new Queue<DirectoryInfo>();
        bool topLevel = true;

        queue.Enqueue(rootDirectory);

        do
        {
            var dirList = queue.Dequeue().GetFileSystemInfos();

            foreach (var entity in dirList)
            {
                if (entity is FileInfo fileInfo)
                {
                    result.Add(fileInfo);
                }
                else if (entity is DirectoryInfo dirInfo)
                {
                    string fileName = entity.Name;

                    if (topLevel && ignoreTopLevelDirectories.Contains(fileName))
                    {
                        continue;
                    }

                    if (ignoreDirectories.Contains(fileName))
                    {
                        continue;
                    }

                    queue.Enqueue(dirInfo);
                }
            }

            topLevel = false;
        } while (queue.Count > 0);

        return result;
    }

    private static void PrintConfig(RawConfig config)
    {
        //Console.WriteLine($" -> fileType: {fileType.name}");
        Console.WriteLine($" -> baseLocale: {config.BaseLocale.TwoLetterISOLanguageName}");
        //Console.WriteLine($" -> fallbackStrategy: {fallbackStrategy.name}");
        Console.WriteLine($" -> inputDirectory: {config.InputDirectory ?? "null(everywhere)"}");
        Console.WriteLine($" -> inputFilePattern: {config.InputFilePattern}");
        // Console.WriteLine(
        //     $" -> outputDirectory: {outputDirectory ?? "null(directory of input)"}");
        // Console.WriteLine($" -> outputFileName: {outputFileName}");
        // Console.WriteLine($" -> outputFileFormat: {outputFormat.name}");
        // Console.WriteLine($" -> localeHandling: {localeHandling}");
        // Console.WriteLine($" -> flutterIntegration: {flutterIntegration}");
        
        // Console.WriteLine($" -> translateVar: {translateVar}");
        // Console.WriteLine($" -> enumName: {enumName}");
        //Console.WriteLine($" -> translationClassVisibility: {translationClassVisibility.name}");
        // Console.WriteLine(
        //     $" -> keyCase: {keyCase != null ? keyCase?.name : "null(no change)"}");
        // Console.WriteLine(
        //     $" -> keyCase (for maps): {keyMapCase != null ? keyMapCase?.name : "null(no change)"}");
        // Console.WriteLine(
        //     $" -> paramCase: {paramCase != null ? paramCase?.name : "null(no change)"}");
       
        // Console.WriteLine($" -> renderFlatMap: {renderFlatMap}");
        // Console.WriteLine($" -> translationOverrides: {translationOverrides}");
        // Console.WriteLine($" -> renderTimestamp: {renderTimestamp}");
        // Console.WriteLine($" -> renderStatistics: {renderStatistics}");
        //Console.WriteLine($" -> maps: {maps}");
        //Console.WriteLine($" -> pluralization/auto: {pluralAuto.name}");
        // Console.WriteLine($" -> pluralization/default_parameter: {pluralParameter}");
        // Console.WriteLine($" -> pluralization/cardinal:{pluralCardinal}");
        // Console.WriteLine($" -> pluralization/ordinal: {pluralOrdinal}");
        // Console.WriteLine($" -> contexts: {contexts.isEmpty ? "no custom contexts": ""}");
        // foreach (var contextType in contexts)
        // {
        //     Console.WriteLine(
        //         "    - ${contextType.enumName} { ${contextType.enumValues?.join(", ") ?? "(inferred)"} }");
        // }

        //Console.WriteLine($" -> interfaces: {interfaces.isEmpty ? "no interfaces" : ""}");
        // for (var inter in interfaces) {
        //     Console.WriteLine("    - ${interface.name}");
        //     Console.WriteLine(
        //         "        Attributes: ${interface.attributes.isEmpty ? "no attributes" : ""}");
        //     for (final a in interface.attributes) {
        //         Console.WriteLine(
        //             "          - ${a.returnType} ${a.attributeName} (${a.parameters.isEmpty ? "no parameters" : a.parameters.map((p) => p.parameterName).join(",
        //             ")})${a.optional ? "(optional)" : ""}");
        //     }
        //     Console.WriteLine("        Paths: ${interface.paths.isEmpty ? "no paths" : ""}");
        //     for (final path in interface.paths) {
        //         Console.WriteLine(
        //             "          - ${path.isContainer ? "children of: " : ""}${path.path}");
        //     }
        // }
        // Console.WriteLine($" -> obfuscation: {obfuscation.enabled ? "enabled" : "disabled"}");
        // Console.WriteLine($" -> imports: {imports}");
    }
}