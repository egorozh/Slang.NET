using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Project2015To2017.Definition;
using Slang.Gpt.Domain.Models;

namespace Slang.CLI.Commands.Translate;

internal static class ConfigRepository
{
    public static GptConfig? GetConfig(Project project, string csProjDirectory)
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

                    if (fileInfo is {Exists: true, Name: "slang.json"})
                    {
                        configJson = File.ReadAllText(fileInfo.FullName);
                        break;
                    }
                }
            }
        }

        if (configJson == null)
            return null;

        var config = JsonSerializer.Deserialize(configJson, GlobalConfigContext.Default.GlobalConfigDto);

        if (config == null)
            return null;

        var model = GptModel.Values.FirstOrDefault(e => e.Id == config.GptConfig?.Model);

        if (model == null)
            throw new Exception(
                $"Unknown model: {config.GptConfig?.Model}\nAvailable models: ${string.Join(", ", GptModel.Values.Select(e => e.Id))}");

        if (string.IsNullOrEmpty(config.GptConfig?.Description))
            throw new Exception("Missing description");

        GptConfig gptConfig = new(
            BaseCulture: new CultureInfo(string.IsNullOrEmpty(config.BaseCulture) ? "en" : config.BaseCulture),
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
    [JsonPropertyName("model")] public string? Model { get; set; }

    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("maxInputLength")] public string? MaxInputLength { get; set; }

    [JsonPropertyName("temperature")] public string? Temperature { get; set; }
}

[JsonSerializable(typeof(GlobalConfigDto))]
[JsonSerializable(typeof(GptConfigDto))]
internal partial class GlobalConfigContext : JsonSerializerContext;