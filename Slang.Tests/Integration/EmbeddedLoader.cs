using System.Reflection;

namespace Slang.Tests.Integration;

internal static class EmbeddedLoader
{
    public static string LoadResource(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(path);

        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }
}