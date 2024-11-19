using System.CommandLine;
using System.Globalization;
using Project2015To2017.Reading;
using Slang.Gpt.Domain.Utils;

namespace Slang.Gpt.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var fileArgument = new Argument<FileInfo?>(
            name: "csproj",
            description: "csproj filepath");

        var apiOption = new Option<string?>(
            name: "--api-key",
            description: "API key");

        var targetOption = new Option<string?>(
            aliases: ["-t", "--target"],
            description: "Target language");

        var fullOption = new Option<bool>(
            aliases: ["-f", "--full"],
            getDefaultValue: () => false,
            description: "Skip partial translation");

        var debugOption = new Option<bool>(
            aliases: ["-d", "--debug"],
            getDefaultValue: () => false,
            description: "Write chat to file");

        var rootCommand = new RootCommand("Running GPT for slang...")
        {
            fileArgument,
            apiOption,
            targetOption,
            fullOption,
            debugOption
        };

        rootCommand.SetHandler(HandleRootCommand,
            fileArgument,
            apiOption,
            targetOption,
            fullOption,
            debugOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleRootCommand(
        FileInfo? csproj,
        string? apiKey,
        string? targetId,
        bool full,
        bool debug)
    {
        Console.WriteLine("Running GPT for slang...");

        if (apiKey == null)
            throw new Exception("Missing API key. Specify it with --api-key=...");

        if (csproj == null)
            throw new Exception("Missing csproj filepath");

        if (!csproj.Exists)
            throw new Exception($"csproj file {csproj} does not exist");

        var reader = new ProjectReader();
        var project = reader.Read(csproj.FullName);

        var gptConfig = ConfigRepository.GetConfig(project, csproj.Directory!.FullName);

        if (gptConfig == null)
        {
            throw new Exception("Missing config");
        }

        List<CultureInfo>? targetLocales = null;

        if (!string.IsNullOrEmpty(targetId))
        {
            var preset = Locales.GetPreset(targetId);
            targetLocales = preset ?? [new CultureInfo(targetId)];

            Console.WriteLine("");
            Console.WriteLine($"Target: {string.Join(", ", targetLocales.Select(e => e.EnglishName))}\n");
        }

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csproj.Directory!.FullName,
            gptConfig.BaseCulture
        );

        await SlangGpt.Execute(httpClient, fileCollection, gptConfig, targetLocales, debug, full);
    }
}