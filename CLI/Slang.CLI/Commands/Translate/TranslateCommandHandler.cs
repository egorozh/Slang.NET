using System.Globalization;
using Project2015To2017.Reading;
using Serilog;
using Serilog.Core;
using Slang.Gpt;
using Slang.Gpt.Domain.Utils;

namespace Slang.CLI.Commands.Translate;

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

            Console.WriteLine();
            Console.WriteLine($"Target: {string.Join(", ", targetLocales.Select(e => e.EnglishName))}");
            Console.WriteLine();
        }

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csProjDirectoryPath,
            gptConfig.BaseCulture
        );
        
        await SlangGpt.Execute(CreateLogger(csProjDirectoryPath, debug), httpClient, fileCollection, gptConfig,
            targetLocales, full);
    }

    private static ILogger CreateLogger(string logDirectory, bool debug)
    {
        string logFilePath = Path.Combine(
            logDirectory,
            "slang_gpt.log"
        );

        var logger = debug
            ? new LoggerConfiguration()
                .WriteTo.File(logFilePath)
                .CreateLogger()
            : Logger.None;

        if (debug)
        {
            Console.WriteLine($"Logs file: {logFilePath}");
            Console.WriteLine();
        }

        return logger;
    }
}