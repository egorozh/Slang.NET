using System.Globalization;
using System.Text;
using Slang.Generator.Generator.Entities;
using Slang.Generator.Nodes.Domain;
using Slang.Generator.Nodes.Nodes;
using Slang.Generator.NodesData;
using static Slang.Generator.Generator.Helper;

namespace Slang.Generator.Generator;

internal static partial class Generator
{
    /// decides which class should be generated
    private record struct ClassTask(string ClassName, ObjectNode Node);

    private static string GenerateTranslations(GenerateConfig config, I18NData localeData)
    {
        Queue<ClassTask> queue = new();
        StringBuilder buffer = new();

        queue.Enqueue(new ClassTask(
            GetClassNameRoot(baseName: config.BaseName, locale: null),
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
            : GetClassNameRoot(baseName: config.BaseName, locale: localeData.Locale);

        // The current class name.
        string varClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassName(parentName: className, locale: localeData.Locale);

        buffer.AppendLineWithTab(
            // The class name of the **base** locale (path-dependent).
            localeData.BaseLocale ? $"partial class {varClassName}" : $"class {varClassName} : {config.ClassName}",
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

        GenerateConstructorInitializers(buffer, localeData, className, node);

        buffer.AppendLineWithTab("}", tabCount: 2);

        buffer.AppendLine();
        buffer.AppendLine("\t// Translations");

        bool prevHasComment = false;

        foreach ((string key, var value) in node.Entries)
        {
            // comment handling
            if (value.Comment != null)
            {
                // add comment add on the line above
                buffer.AppendLine();
                buffer.AppendLine($"\t/// {value.Comment}");
                prevHasComment = true;
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

            buffer.Append('\t');

            bool isOverride = !localeData.BaseLocale;

            string overrideString = isOverride ? "override " : "virtual ";

            if (value is StringTextNode stringTextNode)
            {
                string stringLiteral = GetStringLiteral(
                    stringTextNode.Content, stringTextNode.Links.Count);

                buffer.AppendLine(stringTextNode.Params.Count == 0
                    ? $"public {overrideString}string {key} => {stringLiteral};"
                    : $"public {overrideString}string {key}{_toParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => ${stringLiteral};");
            }
            else if (value is ListNode listNode)
            {
                buffer.Append($"public {overrideString}List<{listNode.GenericType}> {key} => ");
                GenerateList(
                    config: config,
                    @base: localeData.BaseLocale,
                    locale: localeData.Locale,
                    buffer: buffer,
                    queue: queue,
                    className: className,
                    node: listNode,
                    listName: key,
                    depth: 0
                );
            }
            else if (value is ObjectNode objectNode)
            {
                string childClassNoLocale = GetClassName(parentName: className, childName: key);

                if (objectNode.IsMap)
                {
                    // inline map
                    buffer.Append(
                        $"public {overrideString}IReadOnlyDictionary<string, {objectNode.GenericType}> {key} => ");

                    GenerateMap(
                        config: config,
                        @base: localeData.BaseLocale,
                        locale: localeData.Locale,
                        buffer: buffer,
                        queue: queue,
                        className: childClassNoLocale,
                        node: objectNode,
                        depth: 0
                    );
                }
                else
                {
                    // generate a class later on
                    queue.Enqueue(new ClassTask(childClassNoLocale, objectNode));
                    string childClassWithLocale = GetClassName(
                        parentName: className, childName: key, locale: localeData.Locale);
                    buffer.AppendLine(
                        $"public {overrideString}{childClassWithLocale} {key} {{ get; }}");
                }
            }
            else if (value is PluralNode pluralNod)
            {
                buffer.Append($"public {overrideString}string {key}");

                AddPluralCall(
                    buffer: buffer,
                    language: localeData.Locale.TwoLetterISOLanguageName,
                    node: pluralNod,
                    depth: 0
                );
            }
        }
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

        buffer.AppendLineWithTab($"// Path: {node.Path}", tabCount: 1);

        // The root class of **this** locale (path-independent).
        string rootClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassNameRoot(baseName: config.BaseName, locale: localeData.Locale);

        // The current class name.
        string varClassName = GetClassName(parentName: className, locale: localeData.Locale);

        if (localeData.BaseLocale)
        {
            buffer.AppendLineWithTab($"public class {varClassName}", tabCount: 1);
        }
        else
        {
            // The class name of the **base** locale (path-dependent).
            string baseClassName = GetClassName(parentName: className, locale: config.BaseLocale);

            buffer.AppendLineWithTab($"public class {varClassName} : {baseClassName}", tabCount: 1);
        }

        buffer.AppendLineWithTab("{", tabCount: 1);

        // constructor and custom fields
        bool callSuperConstructor = !localeData.BaseLocale &&
                                    config.FallbackStrategy == GenerateFallbackStrategy.BaseLocale;

        buffer.AppendLineWithTab(
            callSuperConstructor
                ? $"public {varClassName}({rootClassName} root) : base(root)"
                : $"public {varClassName}({rootClassName} root)",
            tabCount: 2);

        buffer.AppendLineWithTab("{", tabCount: 2);
        buffer.AppendLineWithTab("this._root = root;", tabCount: 3);

        GenerateConstructorInitializers(buffer, localeData, className, node);

        buffer.AppendLineWithTab("}", tabCount: 2);

        // root
        buffer.AppendLine();

        string overrideOrVirtual = !localeData.BaseLocale ? "override" : "virtual";

        buffer.AppendWithTab($"protected {overrideOrVirtual} {rootClassName} _root {{ get; }}", tabCount: 2);

        buffer.AppendLine(" // ignore: unused_field");

        buffer.AppendLine();
        buffer.AppendLine("\t// Translations");

        bool prevHasComment = false;

        foreach ((string key, var value) in node.Entries)
        {
            // comment handling
            if (value.Comment != null)
            {
                // add comment add on the line above
                buffer.AppendLine();
                buffer.AppendLine($"\t/// {value.Comment}");
                prevHasComment = true;
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

            buffer.Append('\t');

            bool isOverride = !localeData.BaseLocale;

            string overrideString = isOverride ? "override " : "virtual ";

            if (value is StringTextNode stringTextNode)
            {
                string stringLiteral = GetStringLiteral(
                    stringTextNode.Content, stringTextNode.Links.Count);

                buffer.AppendLine(stringTextNode.Params.Count == 0
                    ? $"public {overrideString}string {key} => {stringLiteral};"
                    : $"public {overrideString}string {key}{_toParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => ${stringLiteral};");
            }
            else if (value is ListNode listNode)
            {
                buffer.Append($"public {overrideString}List<{listNode.GenericType}> {key} => ");
                GenerateList(
                    config: config,
                    @base: localeData.BaseLocale,
                    locale: localeData.Locale,
                    buffer: buffer,
                    queue: queue,
                    className: className,
                    node: listNode,
                    listName: key,
                    depth: 0
                );
            }
            else if (value is ObjectNode objectNode)
            {
                string childClassNoLocale = GetClassName(parentName: className, childName: key);

                if (objectNode.IsMap)
                {
                    buffer.Append(
                        $"public {overrideString}IReadOnlyDictionary<string, {objectNode.GenericType}> {key} => ");

                    GenerateMap(
                        config: config,
                        @base: localeData.BaseLocale,
                        locale: localeData.Locale,
                        buffer: buffer,
                        queue: queue,
                        className: childClassNoLocale,
                        node: objectNode,
                        depth: 0
                    );
                }
                else
                {
                    // generate a class later on
                    queue.Enqueue(new ClassTask(childClassNoLocale, objectNode));
                    string childClassWithLocale = GetClassName(
                        parentName: className, childName: key, locale: localeData.Locale);
                    buffer.AppendLine(
                        $"public {overrideString}{childClassWithLocale} {key} {{ get; }}");
                }
            }
            else if (value is PluralNode pluralNod)
            {
                buffer.Append($"public {overrideString}string {key}");

                AddPluralCall(
                    buffer: buffer,
                    language: localeData.Locale.TwoLetterISOLanguageName,
                    node: pluralNod,
                    depth: 0
                );
            }
        }

        buffer.AppendLine("}");
    }

    private static void GenerateConstructorInitializers(
        StringBuilder buffer,
        I18NData localeData,
        string className,
        ObjectNode node)
    {
        foreach ((string key, _) in node.Entries.Where(n => n.Value is ObjectNode {IsMap: false}))
        {
            string childClassWithLocale = GetClassName(
                parentName: className,
                childName: key,
                locale: localeData.Locale);

            buffer.AppendLineWithTab(
                $"{key} = new {childClassWithLocale}(_root);",
                tabCount: 3);
        }
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
        int depth)
    {
        buffer.AppendLine("new Dictionary<string, string> {");

        foreach ((string? key, var value) in node.Entries)
        {
            AddTabs(buffer, depth + 2);

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
                        $"{{\"{key}\", {_toParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => {stringLiteral}}},");
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
                    depth: depth + 1
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
                        depth: depth + 1
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
                    depth: depth + 1
                );
            }
        }

        AddTabs(buffer, depth + 1);

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
        int depth)
    {
        buffer.AppendLine(depth == 0 ? "[" : "new[]{");

        for (int i = 0; i < node.Entries.Count; i++)
        {
            Node value = node.Entries[i];

            AddTabs(buffer, depth + 2);

            // Note:
            // Lists cannot contain rich texts
            // because there is no way to add the "rich" modifier.
            if (value is StringTextNode stringTextNode)
            {
                string stringLiteral = GetStringLiteral(
                    stringTextNode.Content, stringTextNode.Links.Count);

                buffer.AppendLine(stringTextNode.Params.Count == 0
                    ? $"{stringLiteral},"
                    : $"{_toParameterList(stringTextNode.Params, stringTextNode.ParamTypeMap)} => {stringLiteral},");
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
                    depth: depth + 1
                );
            }
            else if (value is ObjectNode objectNode)
            {
                // ignore: prefer_interpolation_to_compose_strings
                string key = $"_{listName ?? ""}_" +
                             depth + "i" + i + "_";
                string childClassNoLocale =
                    GetClassName(parentName: className, childName: key);

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
                        depth: depth + 1
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
                    depth: depth + 1
                );
            }
        }

        AddTabs(buffer, depth + 1);

        buffer.Append(depth == 0 ? "]" : "}");

        buffer.AppendLine(depth == 0 ? ";" : ",");
    }

    /// returns the parameter list
    /// e.g. ({required Object name, required Object age})
    private static string _toParameterList(HashSet<string> @params, Dictionary<string, string> paramTypeMap)
    {
        if (@params.Count == 0)
            return "()";

        StringBuilder buffer = new();

        buffer.Append('(');
        bool first = true;
        foreach (string param in @params)
        {
            if (!first) buffer.Append(", ");

            if (paramTypeMap.ContainsKey(param))
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
        bool forceSemicolon = false
    )
    {
        var textNodeList = node.Quantities.Values.ToList();

        if (textNodeList.Count == 0)
            throw new Exception($"{node.Path} is empty but it is marked for pluralization.");

        // parameters are union sets over all plural forms
        List<string> paramSet = [];
        Dictionary<string, string> paramTypeMap = [];

        foreach (var textNode in textNodeList)
        {
            paramSet.AddRange(textNode.Params);
            paramTypeMap.AddAll(textNode.ParamTypeMap);
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
                depth + 2);

            if (count != node.Quantities.Count - 1)
                buffer.AppendLine(",");

            count++;
        }

        buffer.AppendWithTab(')', depth + 1);

        if (depth == 0 || forceSemicolon)
            buffer.AppendLine(";");
        else
            buffer.AppendLine(",");
    }

    /// Appends count times \t to the buffer
    private static void AddTabs(StringBuilder buffer, int count)
    {
        for (int i = 0; i < count; i++)
            buffer.Append('\t');
    }
}