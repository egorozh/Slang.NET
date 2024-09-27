using System.Globalization;
using Slang.Generator.Data;
using Slang.Generator.Domain.Entities;
using Slang.Generator.Domain.Nodes;
using Slang.Generator.Domain.Nodes.Nodes;

namespace Slang.Generator.Domain.NodesData;

/// <summary>
/// represents one locale and its localized strings
/// </summary>
/// <param name="BaseLocale">whether or not this is the base locale</param>
/// <param name="Locale">the locale (the part after the underscore)</param>
/// <param name="Root">the actual strings</param>
internal record I18NData(
    bool BaseLocale,
    CultureInfo Locale,
    ObjectNode Root
);

internal static class NodesDataRepository
{
    /// Combine all namespaces and build the internal model
    /// The returned locales are sorted (base locale first)
    ///
    /// After this method call, information about the namespace is lost.
    /// It will be just a normal parent.
    public static List<I18NData> GetNodesData(RawConfig rawConfig, TranslationComposition composition)
    {
        var buildConfig = NodesRepository.ToBuildModelConfig(rawConfig);

        KeyValuePair<CultureInfo, Dictionary<string, object?>>? baseEntry = composition
            .FirstOrDefault(entry => Equals(entry.Key, rawConfig.BaseLocale));

        if (!baseEntry.HasValue)
            throw new Exception("Base locale not found");

        // Create the base data first.
        var map = baseEntry.Value.Value;

        var baseResult = NodesRepository.GetNodes(buildConfig, map);

        return composition
            .Select(localeEntry =>
                CreateNodesData(rawConfig,
                    localeEntry.Key,
                    localeEntry.Value,
                    baseResult,
                    buildConfig))
            .ToList();
    }

    private static I18NData CreateNodesData(
        RawConfig rawConfig,
        CultureInfo locale,
        Dictionary<string, object?> map,
        BuildModelResult baseResult,
        BuildModelConfig buildConfig)
    {
        bool @base = Equals(locale, rawConfig.BaseLocale);

        if (@base)
        {
            // Use the already computed base data
            return new I18NData(
                BaseLocale: true,
                Locale: locale,
                Root: baseResult.Root);
        }

        var result = NodesRepository.GetNodes(
            buildConfig: buildConfig,
            map: map,
            baseData: baseResult
        );

        return new I18NData(
            BaseLocale: false,
            Locale: locale,
            Root: result.Root);
    }
}