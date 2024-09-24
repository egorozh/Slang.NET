using System.Globalization;
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
        }

        if (apiKey == null)
            throw new Exception("Missing API key. Specify it with --api-key=...");

        //var gptConfig = GptConfig.fromMap(fileCollection.config.rawMap);
        //todo: getting config from file:
        GptConfig gptConfig = new(
            Model: GptModel.gpt4o_mini,
            Description: "Test application",
            MaxInputLength: GptModel.gpt4o_mini.DefaultInputLength,
            Temperature: null,
            Excludes: []
        );

        if (targetLocales != null)
        {
            Console.WriteLine("");
            Console.WriteLine($"Target: {string.Join(", ", targetLocales.Select(e => e.EnglishName))}\n");
        }

        await SlangGpt.Execute(gptConfig, apiKey, targetLocales, outDir, debug, full);
    }
}