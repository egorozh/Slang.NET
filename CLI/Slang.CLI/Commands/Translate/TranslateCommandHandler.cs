using System.Globalization;
using Project2015To2017.Reading;
using Serilog;
using Serilog.Core;
using Slang.CLI.i18n;
using Slang.Gpt;
using Slang.Gpt.Domain.Models;
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
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string[] projectFiles = Directory.GetFiles(currentDirectory, "*.csproj");

            if (projectFiles.Length == 0)
            {
                Console.WriteLine(texts.CsprojNotFoundInWorkingDir(currentDirectory));
                return;
            }

            csproj = new FileInfo(projectFiles[0]);
        }

        if (!csproj.Exists)
        {
            Console.WriteLine(texts.CsprojNotFound(csproj.FullName));
            return;
        }

        ProjectReader reader = new();
        var project = reader.Read(csproj.FullName);

        string csProjDirectoryPath = csproj.Directory!.FullName;
        var gptConfigResult = ConfigRepository.GetConfig(project, csProjDirectoryPath);

        if (gptConfigResult.TryPickT1(out var error, out var gptConfig))
        {
            ShowGetConfigError(error);
            return;
        }

        var targetLocales = GetTargetCultures(targetId);

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csProjDirectoryPath,
            gptConfig.BaseCulture
        );

        var logger = CreateLogger(csProjDirectoryPath, debug);


        Console.WriteLine(
            texts.GptConfig(gptConfig.Model.Id, gptConfig.MaxInputLength,
                gptConfig.Temperature?.ToString() ?? "default")
        );

        if (gptConfig.Excludes.Count > 0)
        {
            Console.WriteLine(texts.Excludes(string.Join(", ", gptConfig.Excludes.Select(e => e.EnglishName))));
        }

        var res = await SlangGpt.Execute(logger, httpClient, fileCollection, gptConfig,
            startTranslateHandler: (file, targetCulture) =>
            {
                Console.WriteLine();
                Console.WriteLine(texts.StartTranslateLocale(file.Locale, targetCulture, file.FileName));
            },
            partialTranslationHandler: filePath => Console.WriteLine(texts.PartialTranslate(filePath)),
            startRequestHandler: promptCount => Console.WriteLine(texts.StartRequest(promptCount)),
            noNewTranslationsHandler: () => Console.WriteLine(texts.NoNewTranslations),
            endTranslateHandler: targetPath => Console.WriteLine(texts.EndTranslate(targetPath)),
            targetLocales, full);

        ShowResult(gptConfig, res);
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

    private static void ShowGetConfigError(ConfigError error)
    {
        var errorTests = Strings.Loc.Gpt.GetConfigError;

        Console.WriteLine(
            error switch
            {
                ConfigDescriptionMissing => errorTests.MissingDescription,
                ConfigModelNotFound e => errorTests.UnknownModel(e.Model,
                    string.Join(", ", GptModel.Values.Select(info => info.Id))),
                ConfigNotFound => errorTests.ConfigNotFound,
                ConfigNotSerialized => errorTests.ConfigNotSerialized,
                _ => throw new ArgumentOutOfRangeException(nameof(error))
            }
        );
    }

    private static List<CultureInfo>? GetTargetCultures(string? targetId)
    {
        List<CultureInfo>? targetLocales = null;

        if (!string.IsNullOrEmpty(targetId))
        {
            var preset = Locales.GetPreset(targetId);
            targetLocales = preset ?? [new CultureInfo(targetId)];

            Console.WriteLine();
            Console.WriteLine(Strings.Loc.Gpt.TargetCultures(
                    n: targetLocales.Count,
                    culture: targetLocales[0].EnglishName,
                    cultures: string.Join(", ", targetLocales.Select(e => e.EnglishName))
                )
            );
            Console.WriteLine();
        }

        return targetLocales;
    }

    private static void ShowResult(GptConfig gptConfig, GptResult result)
    {
        var texts = Strings.Loc.Gpt.Result;

        Console.WriteLine();
        Console.WriteLine(texts.Summary);
        Console.WriteLine(texts.TotalRequests(result.PromptCount));
        Console.WriteLine(texts.TotalInputTokens(result.InputTokens));
        Console.WriteLine(texts.TotalOutputTokens(result.OutputTokens));

        double totalCost = result.InputTokens * gptConfig.Model.CostPerInputToken +
                           result.OutputTokens * gptConfig.Model.CostPerOutputToken;

        CultureInfo dollarCulture = new("en-US");

        string totalCostString = totalCost.ToString("C6", dollarCulture);
        string perInputTokenCostString = gptConfig.Model.CostPerInputToken.ToString("C8", dollarCulture);
        string perOutputTokenCostString = gptConfig.Model.CostPerOutputToken.ToString("C8", dollarCulture);

        Console.WriteLine(
            texts.TotalCost(
                totalCostString,
                result.InputTokens,
                perInputTokenCostString,
                result.OutputTokens,
                perOutputTokenCostString));
    }
}