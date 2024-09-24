using System.Globalization;

namespace Slang.Generator.Translations.Domain;

/// Contains ALL translations of ALL locales
/// Represented as pure maps without modifications
///
/// locale -> translation map)
public class TranslationComposition : Dictionary<CultureInfo, TranslationsMap>;