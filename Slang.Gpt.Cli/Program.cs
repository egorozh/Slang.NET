using System.Globalization;
using Project2015To2017.Reading;
using Slang.Generator.Data;
using Slang.Generator.SourceGenerator;
using Slang.Gpt.Models;
using Slang.Gpt.Utils;

namespace Slang.Gpt.Cli;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Running GPT for slang...");

        string? apiKey = null;
        List<CultureInfo>? targetLocales = null;
        string? outDir = null;
        bool debug = false;
        bool full = false;
        FileInfo? csprojFileInfo = null;

        foreach (string a in args)
        {
            if (a.StartsWith("--api-key="))
            {
                apiKey = a["--api-key=".Length..];
            }
            else if (a.StartsWith("--target="))
            {
                string id = a["--target=".Length..];
                var preset = Locales.GetPreset(id);
                targetLocales = preset ?? [new CultureInfo(id)];
            }
            else if (a.StartsWith("--outdir="))
            {
                outDir = a["--outdir=".Length..];
            }
            else if (a is "-f" or "--full")
            {
                full = true;
            }
            else if (a is "-d" or "--debug")
            {
                debug = true;
            }
            else if (a.Length > 0)
            {
                csprojFileInfo = new FileInfo(a);

                if (!csprojFileInfo.Exists)
                    throw new Exception($"csproj file {a} does not exist");
            }
        }

        if (apiKey == null)
            throw new Exception("Missing API key. Specify it with --api-key=...");

        if (csprojFileInfo == null)
            throw new Exception("Missing csproj filepath");

        var reader = new ProjectReader();
        var project = reader.Read(csprojFileInfo.FullName);

        string? baseCultureString = null;
        string? modelString = null;
        string? descriptionString = null;
        string? maxInputLengthString = null;
        string? temperatureString = null;

        if (project.PropertyGroups != null)
        {
            foreach (var propertyGroup in project.PropertyGroups)
            {
                foreach (var element in propertyGroup.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "SlangBaseCulture":
                            baseCultureString = element.Value;
                            break;
                        case "SlangModel":
                            modelString = element.Value;
                            break;
                        case "SlangDescription":
                            descriptionString = element.Value;
                            break;
                        case "SlangMaxInputLength":
                            maxInputLengthString = element.Value;
                            break;
                        case "Temperature":
                            temperatureString = element.Value;
                            break;
                    }
                }
            }
        }

        var model = GptModel.Values.FirstOrDefault(e => e.Id == modelString);

        if (model == null)
            throw new Exception(
                $"Unknown model: {modelString}\nAvailable models: ${string.Join(", ", GptModel.Values.Select(e => e.Id))}");

        if (string.IsNullOrEmpty(descriptionString))
            throw new Exception("Missing description");

        var additionalFiles = project.ItemGroups.SelectMany(g => g.Elements())
            .Where(item => item.Name == "AdditionalFiles")
            .ToList();

        List<FileInfo> files = [];

        foreach (var additionalFile in additionalFiles)
        {
            if (additionalFile.HasAttributes)
            {
                var include = additionalFile.Attribute("Include");

                if (include != null)
                {
                    string patternOrFile = include.Value;

                    if (patternOrFile.Contains('*'))
                    {
                        string[] matchedFiles = Directory.GetFiles(csprojFileInfo.Directory!.FullName,
                            patternOrFile.Replace("\\", "/"), SearchOption.AllDirectories);

                        foreach (string file in matchedFiles)
                        {
                            var fileInfo = new FileInfo(file);

                            if (fileInfo.Exists && fileInfo.Name.EndsWith(Constants.AdditionalFilePattern))
                                files.Add(fileInfo);
                        }
                    }
                    else
                    {
                        string filePath = Path.Combine(csprojFileInfo.Directory!.FullName, patternOrFile);

                        var fileInfo = new FileInfo(filePath);

                        if (fileInfo.Exists && fileInfo.Name.EndsWith(Constants.AdditionalFilePattern))
                            files.Add(fileInfo);
                    }
                }
            }
        }

        GptConfig gptConfig = new(
            BaseCulture: new CultureInfo(string.IsNullOrEmpty(baseCultureString) ? "en" : baseCultureString),
            Model: model,
            Description: descriptionString,
            MaxInputLength: !int.TryParse(maxInputLengthString, out int maxInputLength)
                ? GptModel.DefaultInputLength
                : maxInputLength,
            Temperature: double.TryParse(temperatureString, out double temperature) ? temperature : null,
            Excludes: []
        );

        if (targetLocales != null)
        {
            Console.WriteLine("");
            Console.WriteLine($"Target: {string.Join(", ", targetLocales.Select(e => e.EnglishName))}\n");
        }

        var fileCollection = FilesRepository.GetFileCollection(
            baseCulture: gptConfig.BaseCulture,
            allFiles: files);

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        
        await SlangGpt.Execute(httpClient, fileCollection, gptConfig, targetLocales, debug, full);
    }
}