using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using OneOf;
using Slang.Desktop.Features.Project.Domain;

namespace Slang.Desktop.Features.Project.Data;

internal abstract class ConfigError;

internal class ConfigNotFound : ConfigError;

internal class ConfigNotSerialized : ConfigError;

internal class ConfigModelNotFound(string? model) : ConfigError
{
    public string? Model { get; } = model;
}

internal class ConfigDescriptionMissing : ConfigError;

[GenerateOneOf]
internal partial class ConfigResult : OneOfBase<SlangConfig, ConfigError>;

internal static class ConfigRepository
{
    public static ConfigResult GetConfig(Project2015To2017.Definition.Project project, string csProjDirectory)
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

        if (string.IsNullOrEmpty(config.GptConfig?.Description))
            return new ConfigDescriptionMissing();

        string baseCulture = string.IsNullOrEmpty(config.BaseCulture) ? "en" : config.BaseCulture;

        return new SlangConfig(new CultureInfo(baseCulture));
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