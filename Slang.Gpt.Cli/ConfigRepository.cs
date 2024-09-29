using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Project2015To2017.Definition;
using Slang.Gpt.Models;

namespace Slang.Gpt.Cli;

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

        var model = GptModel.Values.FirstOrDefault(e => e.Id == config.gpt.model);

        if (model == null)
            throw new Exception(
                $"Unknown model: {config.gpt.model}\nAvailable models: ${string.Join(", ", GptModel.Values.Select(e => e.Id))}");

        if (string.IsNullOrEmpty(config.gpt.description))
            throw new Exception("Missing description");

        GptConfig gptConfig = new(
            BaseCulture: new CultureInfo(string.IsNullOrEmpty(config.base_culture) ? "en" : config.base_culture),
            Model: model,
            Description: config.gpt.description,
            MaxInputLength: !int.TryParse(config.gpt.maxInputLength, out int maxInputLength)
                ? GptModel.DefaultInputLength
                : maxInputLength,
            Temperature: double.TryParse(config.gpt.temperature, out double temperature) ? temperature : null,
            Excludes: []
        );

        return gptConfig;
    }
}

internal record GlobalConfigDto(
    string? base_culture,
    GptConfigDto gpt
);

internal record GptConfigDto(
    string? model,
    string? description,
    string? maxInputLength,
    string? temperature
);

[JsonSerializable(typeof(GlobalConfigDto))]
[JsonSerializable(typeof(GptConfigDto))]
internal partial class GlobalConfigContext : JsonSerializerContext;