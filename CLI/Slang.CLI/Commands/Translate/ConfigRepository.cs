using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneOf;
using Project2015To2017.Definition;
using Slang.Gpt.Domain.Models;

namespace Slang.CLI.Commands.Translate;

internal abstract class ConfigError;

internal class ConfigNotFound : ConfigError;

internal class ConfigNotSerialized : ConfigError;

internal class ConfigModelNotFound(string? model) : ConfigError
{
    public string? Model { get; } = model;
}

internal class ConfigDescriptionMissing : ConfigError;

[GenerateOneOf]
internal partial class ConfigResult : OneOfBase<GptConfig, ConfigError>;

internal static class ConfigRepository
{
    public static ConfigResult GetConfig(Project project, string csProjDirectory)
    {
        var additionalFiles = project.ItemGroups.SelectMany(g => g.Elements())
            .Where(item => item.Name == "AdditionalFiles")
            .ToList();

        string? configJson = null;

        foreach (var additionalFile in additionalFiles)
        {
            if (additionalFile.HasAttributes)
            {
                var include = additionalFile.Attribute("Include");

                if (include != null)
                {
                    string jsonFileName = include.Value;

                    string filePath = Path.Combine(csProjDirectory, jsonFileName);

                    var fileInfo = new FileInfo(filePath);

                    if (fileInfo is { Exists: true, Name: "slang.json" })
                    {
                        configJson = File.ReadAllText(fileInfo.FullName);
                        break;
                    }
                }
            }
        }

        if (configJson == null)
            return new ConfigNotFound();

        var config = JsonSerializer.Deserialize(configJson, GlobalConfigContext.Default.GlobalConfigDto);

        if (config == null)
            return new ConfigNotSerialized();

        var model = GptModel.Values.FirstOrDefault(e => e.Id == config.GptConfig?.Model);

        if (model == null)
            return new ConfigModelNotFound(config.GptConfig?.Model);

        if (string.IsNullOrEmpty(config.GptConfig?.Description))
            return new ConfigDescriptionMissing();

        string baseCulture = string.IsNullOrEmpty(config.GptConfig.BaseCulture)
            ? string.IsNullOrEmpty(config.BaseCulture) ? "en" : config.BaseCulture
            : config.GptConfig.BaseCulture;

        GptConfig gptConfig = new(
            BaseCulture: new CultureInfo(baseCulture),
            Model: model,
            Description: config.GptConfig.Description,
            MaxInputLength: !int.TryParse(config.GptConfig.MaxInputLength, out int maxInputLength)
                ? GptModel.DefaultInputLength
                : maxInputLength,
            Temperature: double.TryParse(config.GptConfig.Temperature, out double temperature) ? temperature : null,
            Excludes: []
        );

        return gptConfig;
    }
}

internal record GlobalConfigDto
{
    [JsonPropertyName("base_culture")] public string? BaseCulture { get; set; }

    [JsonPropertyName("gpt")] public GptConfigDto? GptConfig { get; set; }
}

internal record GptConfigDto
{
    [JsonPropertyName("base_culture")] public string? BaseCulture { get; set; }

    [JsonPropertyName("model")] public string? Model { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("maxInputLength")] public string? MaxInputLength { get; set; }

    [JsonPropertyName("temperature")] public string? Temperature { get; set; }
}

[JsonSerializable(typeof(GlobalConfigDto))]
[JsonSerializable(typeof(GptConfigDto))]
internal partial class GlobalConfigContext : JsonSerializerContext;