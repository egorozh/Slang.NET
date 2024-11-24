using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Project2015To2017.Definition;
using Slang.Gpt.Domain.Models;

namespace Slang.Gpt.Cli.Commands.Translate;

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

                    if (fileInfo is { Exists: true, Name: "slang.json" })
                    {
                        configJson = File.ReadAllText(fileInfo.FullName);
                        break;
                    }
                }
            }
        }

        if (configJson == null)
            return null;

#if NET6_0
        var config = JsonSerializer.Deserialize<GlobalConfigDto>(configJson);
#else
        var config = JsonSerializer.Deserialize(configJson, GlobalConfigContext.Default.GlobalConfigDto);
#endif

        if (config == null)
            return null;

        var model = GptModel.Values.FirstOrDefault(e => e.Id == config.GptConfig.Model);

        if (model == null)
            throw new Exception(
                $"Unknown model: {config.GptConfig.Model}\nAvailable models: ${string.Join(", ", GptModel.Values.Select(e => e.Id))}");

        if (string.IsNullOrEmpty(config.GptConfig.Description))
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

internal record GlobalConfigDto(
    [property: JsonPropertyName("base_culture")]
    string? BaseCulture,
    [property: JsonPropertyName("gpt")] GptConfigDto GptConfig
);

internal record GptConfigDto(
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("maxInputLength")]
    string? MaxInputLength,
    [property: JsonPropertyName("temperature")]
    string? Temperature
);

#if(NET7_0_OR_GREATER)
[JsonSerializable(typeof(GlobalConfigDto))]
[JsonSerializable(typeof(GptConfigDto))]
internal partial class GlobalConfigContext : JsonSerializerContext;
#endif