using System.Globalization;
using Slang.Generator.Config.Data.Models;
using Slang.Generator.Config.Domain.Entities;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Slang.Generator.Config.Data.Repository;

internal static class ConfigRepository
{
    private const string DefaultBaseLocale = "en";
    private const FallbackStrategy DefaultFallbackStrategy = FallbackStrategy.none;
    private const string? DefaultInputDirectory = null;
    private const string DefaultInputFilePattern = ".i18n.json";
    private const string DefaultPluralParameter = "n";
    private const PluralAuto DefaultPluralAuto = PluralAuto.cardinal;

    /// Parses the full build.yaml file to get the config
    /// May return null if no config entry is found.
    public static RawConfig? FromYaml(string rawYaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance) // see height_in_inches in sample yml 
            .IgnoreUnmatchedProperties()
            .Build();

        try
        {
            var configData = deserializer.Deserialize<ConfigDto>(rawYaml);

            return FromMap(configData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }

    public static RawConfig CreateDefaultConfig()
    {
        return new RawConfig(
            BaseLocale: new CultureInfo(DefaultBaseLocale),
            FallbackStrategy: DefaultFallbackStrategy,
            InputDirectory: DefaultInputDirectory,
            InputFilePattern: DefaultInputFilePattern,
            PluralAuto: DefaultPluralAuto,
            PluralParameter: DefaultPluralParameter
        );
    }

    /// Parses the config entry
    private static RawConfig FromMap(ConfigDto data)
    {
        return new RawConfig(
            BaseLocale: new CultureInfo(data.base_locale ?? DefaultBaseLocale),
            FallbackStrategy: data.fallback_strategy?.ToFallbackStrategy() ?? DefaultFallbackStrategy,
            InputDirectory: RemoveTrailingSlash(data.input_directory) ?? DefaultInputDirectory,
            InputFilePattern: data.input_file_pattern ?? DefaultInputFilePattern,
            PluralAuto: data.pluralization?.auto?.ToPluralAuto() ?? DefaultPluralAuto,
            PluralParameter: data.pluralization?.default_parameter ?? DefaultPluralParameter
        );
    }

    private static string? RemoveTrailingSlash(string? path)
        => path?.Trim().TrimEnd('/').TrimEnd('\\');
}