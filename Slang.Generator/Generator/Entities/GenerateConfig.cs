using System.Globalization;
using Slang.Generator.Config.Domain.Entities;
using Slang.Generator.Nodes.Domain;

namespace Slang.Generator.Generator.Entities;

/// <summary>
/// Config for the generation step (generate dart-content from model)
/// Applies to all locales
/// </summary>
internal record GenerateConfig(
    string Namespace,
    string BaseName, // name of all i18n files, like strings or messages
    CultureInfo BaseLocale, // defaults to 'en'
    GenerateFallbackStrategy FallbackStrategy,
    string ClassName);