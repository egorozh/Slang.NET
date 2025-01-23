using System.CommandLine;
using Slang.CLI.i18n;

namespace Slang.CLI.Commands.Migrate;

internal sealed class MigrateCommand : Command
{
    public MigrateCommand()
        : base("migrate", Strings.Loc.Migrate.Description)
    {
        Argument<FileInfo> resxPathArgument = new(name: "*.resx", description: "Path to the *.resx file");

        Option<FileInfo?> destinationPathOption = new(
            aliases: ["-d", "--dest"],
            description: "Destination path for the generated *.i18n.json file");

        Add(resxPathArgument);
        Add(destinationPathOption);

        this.SetHandler(MigrateCommandHandler.Handle,
            resxPathArgument,
            destinationPathOption);
    }
}