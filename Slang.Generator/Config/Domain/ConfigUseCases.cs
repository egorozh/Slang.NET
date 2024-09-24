using Slang.Generator.Config.Data.Repository;
using Slang.Generator.Config.Domain.Entities;

namespace Slang.Generator.Config.Domain;

public static class ConfigUseCases
{
    public static RawConfig GetRawConfig(string yaml)
    {
        var config = ConfigRepository.FromYaml(yaml) ?? ConfigRepository.CreateDefaultConfig();
        
        return config;
    }

    public static RawConfig? GetRawConfigOrDefault(string yaml)
    {
        var config = ConfigRepository.FromYaml(yaml);
        
        return config;
    }

    public static RawConfig CreateDefaultConfig() => ConfigRepository.CreateDefaultConfig();
}