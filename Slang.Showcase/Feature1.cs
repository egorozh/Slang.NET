namespace Slang.Showcase.MyNamespace;

[Translations(InputFileName = "feature1")]
internal partial class Feature1
{
    /// Title of the day in the schedule of visits
    ///
    /// In ru, this message translates to:
    /// **'{date}'**
    //  @override
    // String schedule_of_visits_day_title(DateTime date) {
    //     final intl.DateFormat dateDateFormat = intl.DateFormat('dd MMMM', localeName);
    //     final String dateString = dateDateFormat.format(date);
    //
    //     return '$dateString';
    // }
    
    
    
    /// No description provided for @profile_rating.
    ///
    /// In ru, this message translates to:
    /// **'Мой рейтинг: {rating}'**
    //String profile_rating(double rating);
    // @override
    //     String profile_rating(double rating) {
    //     final intl.NumberFormat ratingNumberFormat = intl.NumberFormat.decimalPatternDigits(
    //         locale: localeName,
    //         decimalDigits: 2
    //     );
    //     final String ratingString = ratingNumberFormat.format(rating);
    //
    //     return 'Мой рейтинг: $ratingString';
    // }
}

// One message parameter: one placeholder from an @foo entry in the template ARB file.
//
// Placeholders are specified as a JSON map with one entry for each placeholder.
// One placeholder must be specified for each message "{parameter}".
// Each placeholder entry is also a JSON map. If the map is empty, the placeholder
// is assumed to be an Object value whose toString() value will be displayed.
// For example:
//
// "greeting": "{hello} {world}",
// "@greeting": {
//   "description": "A message with a two parameters",
//   "placeholders": {
//     "hello": {},
//     "world": {}
//   }
// }
//
// Each placeholder can optionally specify a valid Dart type. If the type
// is NumberFormat or DateFormat then a format which matches one of the
// type's factory constructors can also be specified. In this example the
// date placeholder is to be formatted with DateFormat.yMMMMd:
//
// "helloWorldOn": "Hello World on {date}",
// "@helloWorldOn": {
//   "description": "A message with a date parameter",
//   "placeholders": {
//     "date": {
//       "type": "DateTime",
//       "format": "yMMMMd"
//     }
//   }
// }
//
// class Placeholder {
//   Placeholder(this.resourceId, this.name, Map<String, Object?> attributes)
//     : example = _stringAttribute(resourceId, name, attributes, 'example'),
//       type = _stringAttribute(resourceId, name, attributes, 'type'),
//       format = _stringAttribute(resourceId, name, attributes, 'format'),
//       optionalParameters = _optionalParameters(resourceId, name, attributes),
//       isCustomDateFormat = _boolAttribute(resourceId, name, attributes, 'isCustomDateFormat');
//
//   final String resourceId;
//   final String name;
//   final String? example;
//   final String? format;
//   final List<OptionalParameter> optionalParameters;
//   final bool? isCustomDateFormat;
//   // The following will be initialized after all messages are parsed in the Message constructor.
//   String? type;
//   bool isPlural = false;
//   bool isSelect = false;
//   bool isDateTime = false;
//   bool requiresDateFormatting = false;
//
//   bool get requiresFormatting => requiresDateFormatting || requiresNumFormatting;
//   bool get requiresNumFormatting => <String>['int', 'num', 'double'].contains(type) && format != null;
//   bool get hasValidNumberFormat => _validNumberFormats.contains(format);
//   bool get hasNumberFormatWithParameters => _numberFormatsWithNamedParameters.contains(format);
//   bool get hasValidDateFormat => validDateFormats.contains(format);
//
//   static String? _stringAttribute(
//     String resourceId,
//     String name,
//     Map<String, Object?> attributes,
//     String attributeName,
//   ) {
//     final Object? value = attributes[attributeName];
//     if (value == null) {
//       return null;
//     }
//     if (value is! String || value.isEmpty) {
//       throw L10nException(
//         'The "$attributeName" value of the "$name" placeholder in message $resourceId '
//         'must be a non-empty string.',
//       );
//     }
//     return value;
//   }
//
//   static bool? _boolAttribute(
//       String resourceId,
//       String name,
//       Map<String, Object?> attributes,
//       String attributeName,
//       ) {
//     final Object? value = attributes[attributeName];
//     if (value == null) {
//       return null;
//     }
//     if (value is bool) {
//       return value;
//     }
//     if (value != 'true' && value != 'false') {
//       throw L10nException(
//         'The "$attributeName" value of the "$name" placeholder in message $resourceId '
//             'must be a boolean value.',
//       );
//     }
//     return value == 'true';
//   }
//
//   static List<OptionalParameter> _optionalParameters(
//     String resourceId,
//     String name,
//     Map<String, Object?> attributes
//   ) {
//     final Object? value = attributes['optionalParameters'];
//     if (value == null) {
//       return <OptionalParameter>[];
//     }
//     if (value is! Map<String, Object?>) {
//       throw L10nException(
//         'The "optionalParameters" value of the "$name" placeholder in message '
//         '$resourceId is not a properly formatted Map. Ensure that it is a map '
//         'with keys that are strings.'
//       );
//     }
//     final Map<String, Object?> optionalParameterMap = value;
//     return optionalParameterMap.keys.map<OptionalParameter>((String parameterName) {
//       return OptionalParameter(parameterName, optionalParameterMap[parameterName]!);
//     }).toList();
//   }
// }


// All translations for a given message specified by a resource id.
//
// The template ARB file must contain an entry called @myResourceId for each
// message named myResourceId. The @ entry describes message parameters
// called "placeholders" and can include an optional description.
// Here's a simple example message with no parameters:
//
// "helloWorld": "Hello World",
// "@helloWorld": {
//   "description": "The conventional newborn programmer greeting"
// }
//
// The value of this Message is "Hello World". The Message's value is the
// localized string to be shown for the template ARB file's locale.
// The docs for the Placeholder explain how placeholder entries are defined.
// class Message {
//   Message(
//     AppResourceBundle templateBundle,
//     AppResourceBundleCollection allBundles,
//     this.resourceId,
//     bool isResourceAttributeRequired,
//     {
//       this.useRelaxedSyntax = false,
//       this.useEscaping = false,
//       this.logger,
//     }
//   ) : assert(resourceId.isNotEmpty),
//       value = _value(templateBundle.resources, resourceId),
//       description = _description(templateBundle.resources, resourceId, isResourceAttributeRequired),
//       placeholders = _placeholders(templateBundle.resources, resourceId, isResourceAttributeRequired),
//       messages = <LocaleInfo, String?>{},
//       parsedMessages = <LocaleInfo, Node?>{} {
//     // Filenames for error handling.
//     final Map<LocaleInfo, String> filenames = <LocaleInfo, String>{};
//     // Collect all translations from allBundles and parse them.
//     for (final AppResourceBundle bundle in allBundles.bundles) {
//       filenames[bundle.locale] = bundle.file.basename;
//       final String? translation = bundle.translationFor(resourceId);
//       messages[bundle.locale] = translation;
//       List<String>? validPlaceholders;
//       if (useRelaxedSyntax) {
//         validPlaceholders = placeholders.entries.map((MapEntry<String, Placeholder> e) => e.key).toList();
//       }
//       try {
//         parsedMessages[bundle.locale] = translation == null ? null : Parser(
//           resourceId,
//           bundle.file.basename,
//           translation,
//           useEscaping: useEscaping,
//           placeholders: validPlaceholders,
//           logger: logger,
//         ).parse();
//       } on L10nParserException catch (error) {
//         logger?.printError(error.toString());
//         // Treat it as an untranslated message in case we can't parse.
//         parsedMessages[bundle.locale] = null;
//         hadErrors = true;
//       }
//     }
//     // Infer the placeholders
//     _inferPlaceholders(filenames);
//   }
//
//   final String resourceId;
//   final String value;
//   final String? description;
//   late final Map<LocaleInfo, String?> messages;
//   final Map<LocaleInfo, Node?> parsedMessages;
//   final Map<String, Placeholder> placeholders;
//   final bool useEscaping;
//   final bool useRelaxedSyntax;
//   final Logger? logger;
//   bool hadErrors = false;
//
//   bool get placeholdersRequireFormatting => placeholders.values.any((Placeholder p) => p.requiresFormatting);
//
//   static String _value(Map<String, Object?> bundle, String resourceId) {
//     final Object? value = bundle[resourceId];
//     if (value == null) {
//       throw L10nException('A value for resource "$resourceId" was not found.');
//     }
//     if (value is! String) {
//       throw L10nException('The value of "$resourceId" is not a string.');
//     }
//     return value;
//   }
//
//   static Map<String, Object?>? _attributes(
//     Map<String, Object?> bundle,
//     String resourceId,
//     bool isResourceAttributeRequired,
//   ) {
//     final Object? attributes = bundle['@$resourceId'];
//     if (isResourceAttributeRequired) {
//       if (attributes == null) {
//         throw L10nException(
//           'Resource attribute "@$resourceId" was not found. Please '
//           'ensure that each resource has a corresponding @resource.'
//         );
//       }
//     }
//
//     if (attributes != null && attributes is! Map<String, Object?>) {
//       throw L10nException(
//         'The resource attribute "@$resourceId" is not a properly formatted Map. '
//         'Ensure that it is a map with keys that are strings.'
//       );
//     }
//
//     return attributes as Map<String, Object?>?;
//   }
//
//   static String? _description(
//     Map<String, Object?> bundle,
//     String resourceId,
//     bool isResourceAttributeRequired,
//   ) {
//     final Map<String, Object?>? resourceAttributes = _attributes(bundle, resourceId, isResourceAttributeRequired);
//     if (resourceAttributes == null) {
//       return null;
//     }
//
//     final Object? value = resourceAttributes['description'];
//     if (value == null) {
//       return null;
//     }
//     if (value is! String) {
//       throw L10nException(
//         'The description for "@$resourceId" is not a properly formatted String.'
//       );
//     }
//     return value;
//   }
//
//   static Map<String, Placeholder> _placeholders(
//     Map<String, Object?> bundle,
//     String resourceId,
//     bool isResourceAttributeRequired,
//   ) {
//     final Map<String, Object?>? resourceAttributes = _attributes(bundle, resourceId, isResourceAttributeRequired);
//     if (resourceAttributes == null) {
//       return <String, Placeholder>{};
//     }
//     final Object? allPlaceholdersMap = resourceAttributes['placeholders'];
//     if (allPlaceholdersMap == null) {
//       return <String, Placeholder>{};
//     }
//     if (allPlaceholdersMap is! Map<String, Object?>) {
//       throw L10nException(
//         'The "placeholders" attribute for message $resourceId, is not '
//         'properly formatted. Ensure that it is a map with string valued keys.'
//       );
//     }
//     return Map<String, Placeholder>.fromEntries(
//       allPlaceholdersMap.keys.map((String placeholderName) {
//         final Object? value = allPlaceholdersMap[placeholderName];
//         if (value is! Map<String, Object?>) {
//           throw L10nException(
//             'The value of the "$placeholderName" placeholder attribute for message '
//             '"$resourceId", is not properly formatted. Ensure that it is a map '
//             'with string valued keys.'
//           );
//         }
//         return MapEntry<String, Placeholder>(placeholderName, Placeholder(resourceId, placeholderName, value));
//       }),
//     );
//   }
//
//   // Using parsed translations, attempt to infer types of placeholders used by plurals and selects.
//   // For undeclared placeholders, create a new placeholder.
//   void _inferPlaceholders(Map<LocaleInfo, String> filenames) {
//     // We keep the undeclared placeholders separate so that we can sort them alphabetically afterwards.
//     final Map<String, Placeholder> undeclaredPlaceholders = <String, Placeholder>{};
//     // Helper for getting placeholder by name.
//     Placeholder? getPlaceholder(String name) => placeholders[name] ?? undeclaredPlaceholders[name];
//     for (final LocaleInfo locale in parsedMessages.keys) {
//       if (parsedMessages[locale] == null) {
//         continue;
//       }
//       final List<Node> traversalStack = <Node>[parsedMessages[locale]!];
//       while (traversalStack.isNotEmpty) {
//         final Node node = traversalStack.removeLast();
//         if (<ST>[
//           ST.placeholderExpr,
//           ST.pluralExpr,
//           ST.selectExpr,
//           ST.argumentExpr
//         ].contains(node.type)) {
//           final String identifier = node.children[1].value!;
//           Placeholder? placeholder = getPlaceholder(identifier);
//           if (placeholder == null) {
//             placeholder = Placeholder(resourceId, identifier, <String, Object?>{});
//             undeclaredPlaceholders[identifier] = placeholder;
//           }
//           if (node.type == ST.pluralExpr) {
//             placeholder.isPlural = true;
//           } else if (node.type == ST.selectExpr) {
//             placeholder.isSelect = true;
//           } else if (node.type == ST.argumentExpr) {
//             placeholder.isDateTime = true;
//           } else {
//             // Here the node type must be ST.placeholderExpr.
//             // A DateTime placeholder must require date formatting.
//             if (placeholder.type == 'DateTime') {
//               placeholder.requiresDateFormatting = true;
//             }
//           }
//         }
//         traversalStack.addAll(node.children);
//       }
//     }
//     placeholders.addEntries(
//       undeclaredPlaceholders.entries
//         .toList()
//         ..sort((MapEntry<String, Placeholder> p1, MapEntry<String, Placeholder> p2) => p1.key.compareTo(p2.key))
//     );
//
//     bool atMostOneOf(bool x, bool y, bool z) {
//       return x && !y && !z
//         || !x && y && !z
//         || !x && !y && z
//         || !x && !y && !z;
//     }
//
//     for (final Placeholder placeholder in placeholders.values) {
//       if (!atMostOneOf(placeholder.isPlural, placeholder.isDateTime, placeholder.isSelect)) {
//         throw L10nException('Placeholder is used as plural/select/datetime in certain languages.');
//       } else if (placeholder.isPlural) {
//         if (placeholder.type == null) {
//           placeholder.type = 'num';
//         }
//         else if (!<String>['num', 'int'].contains(placeholder.type)) {
//           throw L10nException("Placeholders used in plurals must be of type 'num' or 'int'");
//         }
//       } else if (placeholder.isSelect) {
//         if (placeholder.type == null) {
//           placeholder.type = 'String';
//         } else if (placeholder.type != 'String') {
//           throw L10nException("Placeholders used in selects must be of type 'String'");
//         }
//       } else if (placeholder.isDateTime) {
//         if (placeholder.type == null) {
//           placeholder.type = 'DateTime';
//         } else if (placeholder.type != 'DateTime') {
//           throw L10nException("Placeholders used in datetime expressions much be of type 'DateTime'");
//         }
//       }
//       placeholder.type ??= 'Object';
//     }
//   }
// }