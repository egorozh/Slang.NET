using Slang.Generator.Config.Domain;
using Slang.Generator.Config.Domain.Entities;

namespace Slang.Runner;

public static class Test
{
    public static RawConfig GetConfig(string[] args)
    {
        string configPath = args.Length == 0 ? "slang.yaml" : args[0];

        //todo: getting input info from NET SOURCE GENERATOR
        const string targetDirectory = "/Users/egorozh/RiderProjects/Slang.NET/Slang.Showcase";

        string yaml = File.ReadAllText(Path.Combine(targetDirectory, configPath));

        var config = ConfigUseCases.GetRawConfig(yaml);
        
        return config;
    }
}