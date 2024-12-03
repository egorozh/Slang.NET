using System.CommandLine;

namespace Slang.CLI.Commands.Translate;

internal sealed class GptTranslateCommand : RootCommand
{
    public GptTranslateCommand() : base("Translate locale files to target locale with GPT")
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

        Add(fileArgument);
        Add(apiOption);
        Add(targetOption);
        Add(fullOption);
        Add(debugOption);

        this.SetHandler(TranslateCommandHandler.HandleRootCommand,
            fileArgument,
            apiOption,
            targetOption,
            fullOption,
            debugOption);
    }
}