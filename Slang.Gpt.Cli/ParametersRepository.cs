using System.Globalization;
using Slang.Gpt.Utils;

namespace Slang.Gpt.Cli;

internal static class ParametersRepository
{
    public static void ParseArgs(
        string[] args,
        out string? apiKey,
        out List<CultureInfo>? targetLocales,
        out bool debug,
        out bool full,
        out FileInfo? csprojFileInfo)
    {
        apiKey = null;
        targetLocales = null;
        debug = false;
        full = false;
        csprojFileInfo = null;

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
            else if (a is "-f" or "--full")
            {
                full = true;
            }
            else if (a is "-d" or "--debug")
            {
                debug = true;
            }
            else if (a.Length > 0)
            {
                csprojFileInfo = new FileInfo(a);

                if (!csprojFileInfo.Exists)
                    throw new Exception($"csproj file {a} does not exist");
            }
        }
    }
}