using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Project2015To2017.Reading;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Cli.Commands.Translate;

internal static class TranslateCommandHandler
{
    public static async Task HandleRootCommand(
        FileInfo? csproj,
        string? apiKey,
        string? targetId,
        bool full,
        bool debug)
    {
        Console.WriteLine("Started translate command ...");

        if (apiKey == null)
            throw new Exception("Missing API key. Specify it with --api-key=...");

        if (csproj == null)
            throw new Exception("Missing csproj filepath");

        if (!csproj.Exists)
            throw new Exception($"csproj file {csproj} does not exist");

        ProjectReader reader = new();
        var project = reader.Read(csproj.FullName);

        string csProjDirectoryPath = csproj.Directory!.FullName;
        var gptConfig = ConfigRepository.GetConfig(project, csProjDirectoryPath);

        if (gptConfig == null)
            throw new Exception("Missing config");

        List<CultureInfo>? targetLocales = null;

        if (!string.IsNullOrEmpty(targetId))
        {
            var preset = Locales.GetPreset(targetId);
            targetLocales = preset ?? [new CultureInfo(targetId)];

            Console.WriteLine("");
            Console.WriteLine($"Target: {string.Join(", ", targetLocales.Select(e => e.EnglishName))}");
            Console.WriteLine("");
        }

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csProjDirectoryPath,
            gptConfig.BaseCulture
        );

        string logFilePath = Path.Combine(
            csProjDirectoryPath,
            $"slang_gpt_{DateTime.Now}.log"
        );

        ILogger logger = debug
            ? new FileLogger(logFilePath)
            : NullLogger.Instance;

        if (debug)
        {
            Console.WriteLine($"Logs file: {logFilePath}");
            Console.WriteLine();
        }

        await SlangGpt.Execute(logger, httpClient, fileCollection, gptConfig, targetLocales, full);
    }
}