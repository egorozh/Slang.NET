namespace Slang.Generator.SourceGenerator.Extensions;


internal static class DebugHelper
{
    public static void WriteLog(string? value)
    {
#if DEBUG
        File.WriteAllText(Path.Combine(@"C:\Users\Egorozh\RiderProjects\MSG.Toolkit\Toolkit\MobileToolkit.Android.Generators", "log.txt"), value);
#endif
    }
}