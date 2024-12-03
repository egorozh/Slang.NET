using System.CommandLine;
using Slang.CLI.Commands.Translate;

namespace Slang.CLI;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        GptTranslateCommand gptTranslateCommand = new();

        return await gptTranslateCommand.InvokeAsync(args);
    }
}