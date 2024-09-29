using System.Globalization;
using Project2015To2017.Definition;
using Slang.Gpt.Models;

namespace Slang.Gpt.Cli;

internal static class ConfigRepository
{
    public static GptConfig GetConfig(Project project)
    {
        string? baseCultureString = null;
        string? modelString = null;
        string? descriptionString = null;
        string? maxInputLengthString = null;
        string? temperatureString = null;


        if (project.PropertyGroups != null)
        {
            foreach (var propertyGroup in project.PropertyGroups)
            {
                foreach (var element in propertyGroup.Elements())
                {
                    switch (element.Name.LocalName)
                    {
                        case "SlangBaseCulture":
                            baseCultureString = element.Value;
                            break;
                        case "SlangModel":
                            modelString = element.Value;
                            break;
                        case "SlangDescription":
                            descriptionString = element.Value;
                            break;
                        case "SlangMaxInputLength":
                            maxInputLengthString = element.Value;
                            break;
                        case "SlangTemperature":
                            temperatureString = element.Value;
                            break;
                    }
                }
            }
        }

        var model = GptModel.Values.FirstOrDefault(e => e.Id == modelString);

        if (model == null)
            throw new Exception(
                $"Unknown model: {modelString}\nAvailable models: ${string.Join(", ", GptModel.Values.Select(e => e.Id))}");

        if (string.IsNullOrEmpty(descriptionString))
            throw new Exception("Missing description");
        
        GptConfig gptConfig = new(
            BaseCulture: new CultureInfo(string.IsNullOrEmpty(baseCultureString) ? "en" : baseCultureString),
            Model: model,
            Description: descriptionString,
            MaxInputLength: !int.TryParse(maxInputLengthString, out int maxInputLength)
                ? GptModel.DefaultInputLength
                : maxInputLength,
            Temperature: double.TryParse(temperatureString, out double temperature) ? temperature : null,
            Excludes: []
        );
        return gptConfig;
    }
}