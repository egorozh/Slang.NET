using System.CommandLine;
using Slang.Gpt.Cli.Commands.Translate;

namespace Slang.Gpt.Cli;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        TranslateCommand translateCommand = new();

        return await translateCommand.InvokeAsync(args);
    }
}