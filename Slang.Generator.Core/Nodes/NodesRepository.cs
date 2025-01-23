using Slang.Generator.Core.Entities;
using Slang.Generator.Core.Nodes.Nodes;
using Slang.Shared;

namespace Slang.Generator.Core.Nodes;

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
    public static BuildModelConfig ToBuildModelConfig(PluralAuto pluralAuto, string pluralParameter) => new(
        KeyCase: CaseStyle.Pascal,
        KeyMapCase: CaseStyle.Camel,
        ParamCase: CaseStyle.Camel,
        PluralAuto: pluralAuto,
        PluralParameter: pluralParameter
    );


    /// Builds the i18n model for ONE locale
    ///
    /// The [map] must be of type <see cref="Dictionary{String, Object}"/> and all children may of type
    /// String, num, <see cref="List{Object}"/> or <see cref="Dictionary{String, Object}"/>.
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
            ExtendData: null,
            Modifiers: NodeUtils.Empty,
            Entries: resultNodeTree,
            IsMap: false);

        return new BuildModelResult(Root: root);
    }
}