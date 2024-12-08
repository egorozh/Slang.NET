namespace Slang.CLI.Commands.Migrate;

internal static class MigrateCommandHandler
{
    public static async Task Handle(
        FileInfo resxFile,
        FileInfo? destinationPath)
    {
        Console.WriteLine(resxFile.FullName);
        Console.WriteLine(destinationPath?.FullName);

        Console.WriteLine("In development...");
    }
}