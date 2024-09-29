using Project2015To2017.Reading;

namespace Slang.Gpt.Cli;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Running GPT for slang...");

        ParametersRepository.ParseArgs(args, out string? apiKey, out var targetLocales, out bool debug, out bool full,
            out var csprojFileInfo);

        if (apiKey == null)
            throw new Exception("Missing API key. Specify it with --api-key=...");

        if (csprojFileInfo == null)
            throw new Exception("Missing csproj filepath");

        var reader = new ProjectReader();
        var project = reader.Read(csprojFileInfo.FullName);

        var gptConfig = ConfigRepository.GetConfig(project);

        if (targetLocales != null)
        {
            Console.WriteLine("");
            Console.WriteLine($"Target: {string.Join(", ", targetLocales.Select(e => e.EnglishName))}\n");
        }

        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var fileCollection = AdditionalFilesRepository.GetFileCollection(
            project,
            csprojFileInfo.Directory!.FullName,
            gptConfig.BaseCulture
        );

        await SlangGpt.Execute(httpClient, fileCollection, gptConfig, targetLocales, debug, full);
    }
}