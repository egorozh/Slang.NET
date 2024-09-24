using Slang.Generator.Config.Domain.Entities;

namespace Slang.Generator.Nodes.Domain;

/// Config to generate the model.
/// A subset of [RawConfig].
internal record BuildModelConfig(
    FallbackStrategy FallbackStrategy,
    CaseStyle? KeyCase,
    CaseStyle? KeyMapCase,
    CaseStyle? ParamCase,
    PluralAuto PluralAuto,
    string PluralParameter
);