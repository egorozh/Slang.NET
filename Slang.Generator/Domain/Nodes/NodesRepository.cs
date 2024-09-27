using Slang.Generator.Domain.Entities;
using Slang.Generator.Domain.Nodes.Nodes;
using Slang.Generator.Domain.Nodes.Utils;

namespace Slang.Generator.Domain.Nodes;

/// <param name="Root">the actual strings</param>
internal record struct BuildModelResult(ObjectNode Root);

/// Config to generate the model.
/// A subset of [RawConfig].
internal record BuildModelConfig(
    CaseStyle? KeyCase,
    CaseStyle? KeyMapCase,
    CaseStyle? ParamCase,
    PluralAuto PluralAuto,
    string PluralParameter
);

internal static partial class NodesRepository
{
    public static BuildModelConfig ToBuildModelConfig(RawConfig rawConfig) => new(
        KeyCase: CaseStyle.Pascal,
        KeyMapCase: CaseStyle.Camel,
        ParamCase: CaseStyle.Camel,
        PluralAuto: rawConfig.PluralAuto,
        PluralParameter: rawConfig.PluralParameter
    );


    /// Builds the i18n model for ONE locale
    ///
    /// The [map] must be of type Map<String, dynamic> and all children may of type
    /// String, num, List<dynamic> or Map<String, dynamic>.
    ///
    /// If [baseData] is set and [BuildModelConfig.fallbackStrategy] is [FallbackStrategy.baseLocale],
    /// then the base translations will be added to contexts where the translation is missing.
    ///
    /// [handleLinks] can be set false to ignore links and leave them as is
    /// e.g. ${_root.greet(name: name} will be ${_root.greet}
    /// This is used for "Translation Overrides" where the links are resolved
    /// on invocation.
    ///
    /// [shouldEscapeText] can be set false to ignore escaping of text nodes
    /// e.g. "Let"s go" will be "Let"s go" instead of "Let\"s go".
    /// Similar to [handleLinks], this is used for "Translation Overrides".
    public static BuildModelResult GetNodes(
        BuildModelConfig buildConfig,
        Dictionary<string, object?> map,
        BuildModelResult? baseData = null
    )
    {
        // flat map for leaves (TextNode, PluralNode, ContextNode)
        Dictionary<string, ILeafNode> leavesMap = [];

        // 1st iteration: Build nodes according to given map
        //
        // Linked Translations:
        // They will be tracked but not handled
        // Assumption: They are basic linked translations without parameters
        // Reason: Not all TextNodes are built, so var parameters are unknown
        var resultNodeTree = ParseMapNode(
            parentPath: "",
            curr: map,
            config: buildConfig,
            keyCase: buildConfig.KeyCase,
            leavesMap: leavesMap,
            baseData: baseData
        );

        // 2nd iteration: Handle parameterized linked translations
        //
        // TextNodes with parameterized linked translations are rebuilt with correct parameters.
        ResolveLinks(leavesMap);

        // imaginary root node
        var root = new ObjectNode(
            Path: "",
            Comment: null,
            Modifiers: NodeUtils.Empty,
            Entries: resultNodeTree,
            IsMap: false);

        return new BuildModelResult(Root: root);
    }
}