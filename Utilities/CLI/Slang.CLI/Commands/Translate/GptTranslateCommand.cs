using System.CommandLine;
using Slang.CLI.i18n;

namespace Slang.CLI.Commands.Translate;

internal sealed class GptTranslateCommand : Command
{
    public GptTranslateCommand() : base("gpt", Strings.Loc.Gpt.Description)
    {
        var texts = Strings.Loc.Gpt;

        Option<FileInfo?> fileArgument = new(
            name: "--project",
            description: texts.FilePathOption);

        Option<string> apiOption = new(
            name: "--api-key",
            description: texts.ApiKeyOption)
        {
            IsRequired = true
        };

        Option<string?> targetOption = new(
            aliases: ["-t", "--target"],
            description: texts.TargetOption);

        Option<bool> fullOption = new(
            aliases: ["-f", "--full"],
            getDefaultValue: () => false,
            description: texts.FullOption);

        Option<bool> debugOption = new(
            aliases: ["-d", "--debug"],
            getDefaultValue: () => false,
            description: texts.DebugOption);

        Add(fileArgument);
        Add(apiOption);
        Add(targetOption);
        Add(fullOption);
        Add(debugOption);

        this.SetHandler(TranslateCommandHandler.Handle,
            fileArgument,
            apiOption,
            targetOption,
            fullOption,
            debugOption);
    }
}