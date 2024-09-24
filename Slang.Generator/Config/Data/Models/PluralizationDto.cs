namespace Slang.Generator.Config.Data.Models;

public class PluralizationDto
{
    public string? auto { get; set; }
    public string? default_parameter { get; set; }
    public List<string>? cardinal { get; set; }
    public List<string>? ordinal { get; set; }
}