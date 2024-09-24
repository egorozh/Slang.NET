namespace Slang.Generator.Config.Data.Models;

public class ConfigDto
{
    public string? base_locale { get; set; }
    public string? fallback_strategy { get; set; }
    public string? input_directory { get; set; }
    public string? input_file_pattern { get; set; }
    public PluralizationDto? pluralization { get; set; }
}