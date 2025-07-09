using System.Globalization;
using System.Text;
using Slang.Generator.Core.Generator.Entities;
using Slang.Generator.Core.Nodes.Nodes;
using Slang.Generator.Core.NodesData;
using static Slang.Generator.Core.Generator.Internal.Helper;

namespace Slang.Generator.Core.Generator.Internal;

internal static partial class Generator
{
    /// decides which class should be generated
    private record struct ClassTask(string ClassName, ObjectNode Node);

    private static string GenerateTranslations(GenerateConfig config, I18NData localeData)
    {
        Queue<ClassTask> queue = new();
        StringBuilder buffer = new();

        queue.Enqueue(new ClassTask(
            GetClassNameRoot(baseName: config.ClassName, locale: null),
            localeData.Root
        ));

        // only for the first class
        bool root = true;

        do
        {
            var task = queue.Dequeue();

            if (root)
                GenerateRootClass(buffer, config, localeData, queue, task.ClassName, task.Node);
            else
                GenerateClass(buffer, config, localeData, queue, task.ClassName, task.Node);

            root = false;
        } while (queue.Count > 0);

        return $$"""
                 #nullable enable
                 
                 using Slang;
                 using {{config.Namespace}};
                 using System;
                 using System.Collections.Generic;
                 using System.Globalization;
                 using System.Linq;
                 using System.Threading;

                 namespace {{config.Namespace}}
                 {
                 {{buffer}}
                    }
                 }
                 """;
    }


    /// generates a class and all of its members of ONE locale
    /// adds subclasses to the queue
    private static void GenerateRootClass(
        StringBuilder buffer,
        GenerateConfig config,
        I18NData localeData,
        Queue<ClassTask> queue,
        string className,
        ObjectNode node)
    {
        // The root class of **this** locale (path-independent).
        string rootClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassNameRoot(baseName: config.ClassName, locale: localeData.Locale);

        // The current class name.
        string varClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassName(parentName: className, locale: localeData.Locale);

        buffer.AppendLineWithTab(
            // The class name of the **base** locale (path-dependent).
            localeData.BaseLocale
                ? $"partial class {varClassName}"
                : $"partial class {varClassName} : {config.ClassName}",
            tabCount: 1);

        buffer.AppendWithTab('{', tabCount: 1);

        buffer.AppendLine();

        string overrideOrVirtual = !localeData.BaseLocale ? "override" : "virtual";

        buffer.AppendWithTab($"protected {overrideOrVirtual} {rootClassName} _root {{ get; }}", tabCount: 2);

        buffer.AppendLine(" // ignore: unused_field");

        buffer.AppendLine();
        buffer.AppendLineWithTab($"public {varClassName}()", tabCount: 2);
        buffer.AppendLineWithTab("{", tabCount: 2);

        buffer.AppendLineWithTab("_root = this;", tabCount: 3);

        GenerateConstructorInitializers(buffer, localeData, className, node, tabAnchor: 3);

        buffer.AppendLineWithTab("}", tabCount: 2);

        buffer.AppendLine();
        buffer.AppendLineWithTab("// Translations", tabCount: 2);

        GenerateProperties(buffer, config, localeData, queue, className, node, tabAnchor: 2);
    }

    /// generates a class and all of its members of ONE locale
    /// adds subclasses to the queue
    private static void GenerateClass(StringBuilder buffer, GenerateConfig config,
        I18NData localeData,
        Queue<ClassTask> queue,
        string className,
        ObjectNode node)
    {
        buffer.AppendLine();

        buffer.AppendLineWithTab($"// Path: {node.Path}", tabCount: 2);

        // The root class of **this** locale (path-independent).
        string rootClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassNameRoot(baseName: config.ClassName, locale: localeData.Locale);

        // The current class name.
        string varClassName = GetClassName(parentName: className, locale: localeData.Locale);

        if (localeData.BaseLocale)
        {
            buffer.AppendLineWithTab($"public class {varClassName}", tabCount: 2);
        }
        else
        {
            // The class name of the **base** locale (path-dependent).
            string baseClassName = GetClassName(parentName: className, locale: config.BaseLocale);

            buffer.AppendLineWithTab($"public class {varClassName} : {baseClassName}", tabCount: 2);
        }

        buffer.AppendLineWithTab("{", tabCount: 2);

        // constructor and custom fields
        bool callSuperConstructor = !localeData.BaseLocale;

        buffer.AppendLineWithTab(
            callSuperConstructor
                ? $"public {varClassName}({rootClassName} root) : base(root)"
                : $"public {varClassName}({rootClassName} root)",
            tabCount: 3);

        buffer.AppendLineWithTab("{", tabCount: 3);
        buffer.AppendLineWithTab("this._root = root;", tabCount: 4);

        GenerateConstructorInitializers(buffer, localeData, className, node, tabAnchor: 4);

        buffer.AppendLineWithTab("}", tabCount: 3);

        // root
        buffer.AppendLine();

        string overrideOrVirtual = !localeData.BaseLocale ? "override" : "virtual";

        buffer.AppendWithTab($"protected {overrideOrVirtual} {rootClassName} _root {{ get; }}", tabCount: 3);

        buffer.AppendLine(" // ignore: unused_field");

        buffer.AppendLine();
        buffer.AppendLineWithTab("// Translations", tabCount: 3);

        GenerateProperties(buffer, config, localeData, queue, className, node, tabAnchor: 3);

        buffer.AppendLineWithTab("}", tabCount: 2);
    }

    private static void GenerateProperties(StringBuilder buffer, GenerateConfig config, I18NData localeData,
        Queue<ClassTask> queue,
        string className,
        ObjectNode node,
        int tabAnchor)
    {
        bool prevHasComment = false;

        foreach ((string key, var value) in node.Entries)
        {
            prevHasComment = HandleComment(buffer, value, prevHasComment, tabAnchor, localeData);

            bool isOverride = !localeData.BaseLocale;

            string overrideString = isOverride ? "override " : "virtual ";

            switch (value)
            {
                case StringTextNode stringTextNode:
                    GenerateProperty(buffer, stringTextNode, overrideString, key, tabAnchor);
                    break;
                case ListNode listNode:
                    GenerateListProperty(buffer, config, localeData, queue, className, overrideString, listNode, key,
                        tabAnchor);
                    break;
                case ObjectNode objectNode:
                    GenerateObjectProperty(buffer, config, localeData, queue, className, key, objectNode,
                        overrideString, tabAnchor);
                    break;
                case PluralNode pluralNod:
                    GeneratePluralProperty(buffer, localeData, overrideString, key, pluralNod, tabAnchor);
                    break;
            }
        }
    }

    private static void GeneratePluralProperty(StringBuilder buffer, I18NData localeData, string overrideString,
        string key, PluralNode pluralNod, int tabAnchor)
    {
        buffer.AppendWithTab($"public {overrideString}string {key}", tabCount: tabAnchor);

        AddPluralCall(
            buffer: buffer,
            language: localeData.Locale.TwoLetterISOLanguageName,
            node: pluralNod,
            depth: 0,
            tabAnchor
        );
    }

    private static void GenerateObjectProperty(StringBuilder buffer, GenerateConfig config, I18NData localeData,
        Queue<ClassTask> queue, string className, string key, ObjectNode objectNode, string overrideString,
        int tabAnchor)
    {
        string childClassNoLocale = GetClassName(parentName: className, childName: key);

        if (objectNode.IsMap)
        {
            // inline map
            buffer.AppendWithTab(
                $"public {overrideString}IReadOnlyDictionary<string, {objectNode.GenericType}> {key} => ",
                tabCount: tabAnchor);

            GenerateMap(
                config: config,
                @base: localeData.BaseLocale,
                locale: localeData.Locale,
                buffer: buffer,
                queue: queue,
                className: childClassNoLocale,
                node: objectNode,
                depth: 0,
                tabAnchor: tabAnchor
            );
        }
        else
        {
            // generate a class later on
            queue.Enqueue(new ClassTask(childClassNoLocale, objectNode));
            string childClassWithLocale = GetClassName(
                parentName: className, childName: key, locale: localeData.Locale);
            buffer.AppendLineWithTab(
                $"public {overrideString}{childClassWithLocale} {key} {{ get; }}", tabCount: tabAnchor);
        }
    }

    private static void GenerateListProperty(StringBuilder buffer, GenerateConfig config, I18NData localeData,
        Queue<ClassTask> queue,
        string className, string overrideString, ListNode listNode, string key, int tabAnchor)
    {
        buffer.AppendWithTab($"public {overrideString}List<{listNode.GenericType}> {key} => ", tabCount: tabAnchor);
        GenerateList(
            config: config,
            @base: localeData.BaseLocale,
            locale: localeData.Locale,
            buffer: buffer,
            queue: queue,
            className: className,
            node: listNode,
            listName: key,
            depth: 0,
            tabAnchor: tabAnchor
        );
    }


    private static void GenerateConstructorInitializers(
        StringBuilder buffer,
        I18NData localeData,
        string className,
        ObjectNode node,
        int tabAnchor)
    {
        foreach ((string key, _) in node.Entries.Where(n => n.Value is ObjectNode {IsMap: false}))
        {
            string childClassWithLocale = GetClassName(
                parentName: className,
                childName: key,
                locale: localeData.Locale);

            buffer.AppendLineWithTab(
                $"{key} = new {childClassWithLocale}(_root);",
                tabCount: tabAnchor);
        }
    }

    private static bool HandleComment(
        StringBuilder buffer,
        Node value,
        bool prevHasComment,
        int tabAnchor,
        I18NData localeData)
    {
        string? comment = value.ExtendData?.Description;

        // comment handling
        if (comment != null)
        {
            string[] commentStrings = comment.Split('\n');

            // add comment add on the line above
            buffer.AppendLine();

            foreach (string commentString in commentStrings)
                buffer.AppendLineWithTab($"/// {commentString}", tabCount: tabAnchor);

            if (value is StringTextNode)
                buffer.AppendLineWithTab($"///", tabCount: tabAnchor);
            else
                buffer.AppendLine();

            prevHasComment = true;
        }

        if (value is StringTextNode stringTextNode)
        {
            if (comment == null)
                buffer.AppendLine();

            buffer.AppendLineWithTab(
                $"/// In {localeData.Locale.TwoLetterISOLanguageName}, this message translates to:",
                tabCount: tabAnchor);
            buffer.AppendLineWithTab($"/// **\"{stringTextNode.Content}\"**", tabCount: tabAnchor);
        }
        else
        {
            if (prevHasComment)
            {
                // add a new line to separate from previous entry with comment
                buffer.AppendLine();
            }

            prevHasComment = false;
        }

        return prevHasComment;
    }

    private static void GenerateProperty(StringBuilder buffer, StringTextNode stringTextNode, string overrideString,
        string key, int tabAnchor)
    {
        if (stringTextNode is {ExtendData: { } extendData, Params.Count: > 0})
        {
            GeneratePropertyEx(buffer, stringTextNode, extendData, overrideString, key, tabAnchor);
            return;
        }

        string stringLiteral = GetStringLiteral(stringTextNode.Content, stringTextNode.Links.Count);

        buffer.AppendLineWithTab(stringTextNode.Params.Count == 0
                ? $"public {overrideString}string {key} => {stringLiteral};"
                : $"public {overrideString}string {key}{ToParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => ${stringLiteral};",
            tabCount: tabAnchor);
    }

    private static void GeneratePropertyEx(
        StringBuilder buffer,
        StringTextNode stringTextNode,
        ExtendData extendData,
        string overrideString,
        string key,
        int tabAnchor)
    {
        bool? isAnyFormat = extendData.Placeholders?.Values.Any(p => !string.IsNullOrEmpty(p.Format));

        if (isAnyFormat == true)
        {
            GenerateFormatMethod(buffer, stringTextNode, extendData.Placeholders, overrideString, key, tabAnchor);
            return;
        }

        string stringLiteral = GetStringLiteral(stringTextNode.Content, stringTextNode.Links.Count);

        buffer.AppendLineWithTab(stringTextNode.Params.Count == 0
                ? $"public {overrideString}string {key} => {stringLiteral};"
                : $"public {overrideString}string {key}{ToParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap, extendData.Placeholders)} => ${stringLiteral};",
            tabCount: tabAnchor);
    }

    private static readonly HashSet<string> SupportedFormattingTypes =
    [
        "int",
        "long",
        "double",
        "decimal",
        "float",
        "DateTime",
        "DateOnly",
        "TimeOnly",
        "TimeSpan"
    ];

    private static void GenerateFormatMethod(
        StringBuilder buffer,
        StringTextNode stringTextNode,
        IReadOnlyDictionary<string, Placeholder>? placeholders,
        string overrideString,
        string key,
        int tabAnchor)
    {
        string parameters =
            ToParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap, placeholders);

        buffer.AppendLineWithTab(
            $"public {overrideString}string {key}{parameters}",
            tabCount: tabAnchor);

        buffer.AppendLineWithTab("{", tabCount: tabAnchor);

        string stringLiteral = GetStringLiteral(stringTextNode.Content, stringTextNode.Links.Count);

        if (placeholders != null)
            foreach ((string paramKey, var placeholder) in placeholders)
            {
                if (!string.IsNullOrEmpty(placeholder.Format))
                {
                    string newParam = $"{paramKey}String";

                    stringLiteral = stringLiteral.Replace(paramKey, newParam);

                    string type;

                    if (placeholder.Type is { } s)
                        type = s;
                    else if (stringTextNode.ParamTypeMap.ContainsKey(paramKey))
                        type = stringTextNode.ParamTypeMap[paramKey];
                    else
                        type = "object";

                    buffer.AppendLineWithTab(
                        SupportedFormattingTypes.Contains(type)
                            ? $"string {newParam} = {paramKey}.ToString(\"{placeholder.Format}\");"
                            : $"string {newParam} = string.Format(\"{placeholder.Format}\", {paramKey});",
                        tabCount: tabAnchor + 1);
                }
            }

        buffer.AppendLineWithTab($"return ${stringLiteral};", tabCount: tabAnchor + 1);

        buffer.AppendLineWithTab("}", tabCount: tabAnchor);
    }

    /// generates a map of ONE locale
    /// similar to _generateClass but anonymous and accessible via key
    private static void GenerateMap(
        GenerateConfig config,
        bool @base,
        CultureInfo locale,
        StringBuilder buffer,
        Queue<ClassTask> queue,
        string className, // without locale
        ObjectNode node,
        int depth,
        int tabAnchor)
    {
        buffer.AppendLine("new Dictionary<string, string> {");

        foreach ((string? key, var value) in node.Entries)
        {
            AddTabs(buffer, depth + tabAnchor + 2);

            // Note:
            // Maps cannot contain rich texts
            // because there is no way to add the "rich" modifier.
            if (value is StringTextNode stringTextNode)
            {
                string stringLiteral = GetStringLiteral(
                    stringTextNode.Content, stringTextNode.Links.Count);
                if (stringTextNode.Params.Count == 0)
                {
                    buffer.AppendLine($"{{\"{key}\", {stringLiteral}}},");
                }
                else
                {
                    buffer.AppendLine(
                        $"{{\"{key}\", {ToParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => {stringLiteral}}},");
                }
            }
            else if (value is ListNode listNode)
            {
                buffer.Append($"\"{key}\": ");
                GenerateList(
                    config: config,
                    @base: @base,
                    locale: locale,
                    buffer: buffer,
                    queue: queue,
                    className: className,
                    node: listNode,
                    listName: key,
                    depth: depth + 1,
                    tabAnchor: tabAnchor
                );
            }
            else if (value is ObjectNode objectNode)
            {
                string childClassNoLocale =
                    GetClassName(parentName: className, childName: key);

                if (objectNode.IsMap)
                {
                    // inline map
                    buffer.Append($"\"{key}\": ");
                    GenerateMap(
                        config: config,
                        @base: @base,
                        locale: locale,
                        buffer: buffer,
                        queue: queue,
                        className: childClassNoLocale,
                        node: objectNode,
                        depth: depth + 1,
                        tabAnchor: tabAnchor
                    );
                }
                else
                {
                    // generate a class later on
                    queue.Enqueue(new ClassTask(childClassNoLocale, objectNode));
                    string childClassWithLocale =
                        GetClassName(parentName: className, childName: key, locale: locale);
                    buffer.AppendLine($"\"{key}\": {childClassWithLocale}._(_root),");
                }
            }
            else if (value is PluralNode pluralNode)
            {
                buffer.Append($"\"{key}\": ");
                AddPluralCall(
                    buffer: buffer,
                    language: locale.TwoLetterISOLanguageName,
                    node: pluralNode,
                    depth: depth + 1,
                    tabAnchor: tabAnchor
                );
            }
        }

        AddTabs(buffer, depth + tabAnchor + 1);

        buffer.Append('}');

        buffer.AppendLine(depth == 0 ? ";" : ",");
    }

    /// generates a list
    private static void GenerateList(
        GenerateConfig config,
        bool @base,
        CultureInfo locale,
        StringBuilder buffer,
        Queue<ClassTask> queue,
        string className,
        ListNode node,
        string? listName,
        int depth,
        int tabAnchor)
    {
        buffer.AppendLine(depth == 0 ? "[" : "new[]{");

        for (int i = 0; i < node.Entries.Count; i++)
        {
            Node value = node.Entries[i];

            AddTabs(buffer, depth + tabAnchor + 2);

            // Note:
            // Lists cannot contain rich texts
            // because there is no way to add the "rich" modifier.
            if (value is StringTextNode stringTextNode)
            {
                string stringLiteral = GetStringLiteral(
                    stringTextNode.Content, stringTextNode.Links.Count);

                buffer.AppendLine(stringTextNode.Params.Count == 0
                    ? $"{stringLiteral},"
                    : $"{ToParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => {stringLiteral},");
            }
            else if (value is ListNode listNode)
            {
                GenerateList(
                    config: config,
                    @base: @base,
                    locale: locale,
                    buffer: buffer,
                    queue: queue,
                    className: className,
                    node: listNode,
                    listName: listName,
                    depth: depth + 1,
                    tabAnchor: tabAnchor
                );
            }
            else if (value is ObjectNode objectNode)
            {
                // ignore: prefer_interpolation_to_compose_strings
                string key = $"_{listName ?? ""}_{depth}i{i}_";

                string childClassNoLocale = GetClassName(parentName: className, childName: key);

                if (objectNode.IsMap)
                {
                    // inline map
                    GenerateMap(
                        config: config,
                        @base: @base,
                        locale: locale,
                        buffer: buffer,
                        queue: queue,
                        className: childClassNoLocale,
                        node: objectNode,
                        depth: depth + 1,
                        tabAnchor: tabAnchor
                    );
                }
                else
                {
                    // generate a class later on
                    queue.Enqueue(new ClassTask(childClassNoLocale, objectNode));
                    string childClassWithLocale = GetClassName(
                        parentName: className,
                        childName: key,
                        locale: locale
                    );
                    buffer.AppendLine($"new {childClassWithLocale}(_root),");
                }
            }
            else if (value is PluralNode pluralNode)
            {
                AddPluralCall(
                    buffer: buffer,
                    language: locale.TwoLetterISOLanguageName,
                    node: pluralNode,
                    depth: depth + 1,
                    tabAnchor: tabAnchor
                );
            }
        }

        AddTabs(buffer, depth + 1);

        buffer.Append(depth == 0 ? "]" : "}");

        buffer.AppendLine(depth == 0 ? ";" : ",");
    }

    /// returns the parameter list
    /// e.g. (object name, object age)
    private static string ToParameterList(
        HashSet<string> @params,
        Dictionary<string, string> paramTypeMap,
        IReadOnlyDictionary<string, Placeholder>? placeholders = null)
    {
        if (@params.Count == 0)
            return "()";

        StringBuilder buffer = new();

        buffer.Append('(');
        bool first = true;
        foreach (string param in @params)
        {
            if (!first) buffer.Append(", ");

            if ((placeholders?.ContainsKey(param) ?? false) && placeholders[param].Type is { } type)
                buffer.Append($"{type} ");
            else if (paramTypeMap.ContainsKey(param))
                buffer.Append($"{paramTypeMap[param]} ");
            else
                buffer.Append("object ");

            buffer.Append(param);
            first = false;
        }

        buffer.Append(')');

        return buffer.ToString();
    }

    private static void AddPluralCall(
        StringBuilder buffer,
        string language,
        PluralNode node,
        int depth,
        int tabAnchor,
        bool forceSemicolon = false
    )
    {
        var textNodeList = node.Quantities.Values.ToList();

        if (textNodeList.Count == 0)
            throw new Exception($"{node.Path} is empty but it is marked for pluralization.");

        // parameters are union sets over all plural forms
        List<string> paramSet = [];
        Dictionary<string, string?> paramTypeMap = [];

        foreach (var textNode in textNodeList)
        {
            paramSet.AddRange(textNode.Params);
            paramTypeMap!.AddAll(textNode.ParamTypeMap);
        }

        string builderParam = $"{node.ParamName}Builder";
        var @params = paramSet.Where(p => p != node.ParamName && p != builderParam).ToList();

        // add plural parameter first
        buffer.Append($"({node.ParamType} {node.ParamName}");

        for (int i = 0; i < @params.Count; i++)
        {
            buffer.Append($", {paramTypeMap[@params[i]] ?? "int"} ");
            buffer.Append(@params[i]);
        }

        // custom resolver has precedence
        var prefix = node.PluralType;
        buffer.Append(") => ");

        buffer.AppendLine(
            $"PluralResolvers.{prefix}(\"{language}\")({node.ParamName},");

        int count = 0;

        foreach (var quantity in node.Quantities)
        {
            var textNode = quantity.Value;

            buffer.AppendWithTab(
                $"{quantity.Key.ParamName()}: ${GetStringLiteral(textNode.Content, textNode.Links.Count)}",
                depth + tabAnchor + 2);

            if (count != node.Quantities.Count - 1)
                buffer.AppendLine(",");

            count++;
        }

        buffer.AppendWithTab(')', depth + tabAnchor + 1);

        if (depth == 0 || forceSemicolon)
            buffer.AppendLine(";");
        else
            buffer.AppendLine(",");
    }

    private const char TabChar = '\t';

    /// Appends count times \t to the buffer
    private static void AddTabs(StringBuilder buffer, int count)
    {
        for (int i = 0; i < count; i++)
            buffer.Append(TabChar);
    }
}