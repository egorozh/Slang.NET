using System.Globalization;
using Project2015To2017.Reading;
using Serilog;
using Serilog.Core;
using Slang.CLI.i18n;
using Slang.Gpt;
using Slang.Gpt.Domain.Utils;

namespace Slang.CLI.Commands.Translate;

internal static class TranslateCommandHandler
{
    public static async Task Handle(
        FileInfo? csproj,
        string apiKey,
        string? targetId,
        bool full,
        bool debug)
    {
        var texts = Strings.Loc.Gpt;

        Console.WriteLine(texts.StartTranslate);

        if (apiKey == null)
            throw new Exception("Missing API key. Specify it with --api-key=...");

        if (csproj == null)
            throw new Exception("Missing csproj filepath");

        if (!csproj.Exists)
            Console.WriteLine(texts.CsprojNotFound(csproj.FullName));

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
            Console.WriteLine(texts.TargetCultures(
                    n: targetLocales.Count,
                    culture: targetLocales[0].EnglishName,
                    cultures: string.Join(", ", targetLocales.Select(e => e.EnglishName))
                )
            );

            Console.WriteLine();
        }

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csProjDirectoryPath,
            gptConfig.BaseCulture
        );

        var logger = CreateLogger(csProjDirectoryPath, debug);

        await SlangGpt.Execute(logger, httpClient, fileCollection, gptConfig, targetLocales, full);
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