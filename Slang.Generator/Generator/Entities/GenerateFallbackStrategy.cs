namespace Slang.Generator.Generator.Entities;

/// <summary>
/// Similar to [FallbackStrategy] but [FallbackStrategy.baseLocaleEmptyString]
/// has been already handled in the previous step.
/// </summary>
public enum GenerateFallbackStrategy
{
    None,
    BaseLocale
}