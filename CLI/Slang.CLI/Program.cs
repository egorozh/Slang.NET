﻿using System.CommandLine;
using Slang.CLI.Commands.Translate;
using Slang.CLI.i18n;

namespace Slang.CLI;

internal static class Program
{
    private static Task<int> Main(string[] args)
    {
        RootCommand rootCommand = new(Strings.Instance.Root.Welcome);

        rootCommand.AddCommand(new GptTranslateCommand());

        return rootCommand.InvokeAsync(args);
    }
}