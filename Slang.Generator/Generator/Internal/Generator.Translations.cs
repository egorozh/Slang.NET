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
    private record ClassTask(string ClassName, ObjectNode Node);

    private static string GenerateTranslations(GenerateConfig config, I18NData localeData)
    {
        Queue<ClassTask> queue = new();
        StringBuilder buffer = new();

        buffer.AppendLine(GenerateImports(config.Namespace));

        buffer.AppendLine($"namespace {config.Namespace};");

        queue.Enqueue(new ClassTask(
            GetClassNameRoot(
                baseName: config.BaseName,
                locale: null
                //config.translationClassVisibility
            ),
            localeData.Root
        ));

        // only for the first class
        bool root = true;

        do
        {
            var task = queue.Dequeue();

            string code = root
                ? _generateRootClass(config, localeData, queue, task.ClassName, task.Node)
                : _generateClass(config, localeData, queue, task.ClassName, task.Node);

            if (root)
                buffer.AppendLine(code);
            else
            {
                buffer.AppendLine(string.Join(Environment.NewLine,
                    code.Split(Environment.NewLine).Select(s => s.Insert(0, "\t"))));
            }

            root = false;
        } while (queue.Count > 0);

        buffer.AppendLine("}");

        return buffer.ToString();
    }


    /// generates a class and all of its members of ONE locale
    /// adds subclasses to the queue
    private static string _generateRootClass(
        GenerateConfig config,
        I18NData localeData,
        Queue<ClassTask> queue,
        string className,
        ObjectNode node
    )
    {
        StringBuilder buffer = new();

        buffer.AppendLine();

        // The root class of **this** locale (path-independent).
        string rootClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassNameRoot(
                baseName: config.BaseName,
                locale: localeData.Locale
            );

        // The current class name.
        string varClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassName(
                parentName: className,
                locale: localeData.Locale
            );

        if (localeData.BaseLocale)
        {
            buffer.AppendLine(
                $"partial class {varClassName}");
            buffer.AppendLine("{");
        }
        else
        {
            // The class name of the **base** locale (path-dependent).
            string baseClassName = config.ClassName;

            if (config.FallbackStrategy == GenerateFallbackStrategy.None)
            {
                buffer.AppendLine(
                    $"class {varClassName} : {baseClassName}");
                buffer.AppendLine("{");
            }
            else
            {
                buffer.AppendLine($"class {varClassName} : {baseClassName}");
                buffer.AppendLine("{");
            }
        }

        buffer.AppendLine();

        buffer.AppendLine();

        string overrideOrVirtual = !localeData.BaseLocale ? "override" : "virtual";

        buffer.Append($"\tprotected {overrideOrVirtual} {rootClassName} _root {{ get; }}");

        buffer.AppendLine(" // ignore: unused_field");

        buffer.AppendLine();
        buffer.AppendLine($"\tpublic {varClassName}()");
        buffer.AppendLine("\t{");

        buffer.AppendLine("\t\t_root = this;");

        foreach ((string key, var value) in node.Entries)
        {
            if (value is ObjectNode objectNode)
            {
                if (!objectNode.IsMap)
                {
                    string childClassWithLocale = GetClassName(
                        parentName: className, childName: key, locale: localeData.Locale);
                    buffer.AppendLine(
                        $"\t\t{key} = new {childClassWithLocale}(_root);");
                }
            }
        }


        buffer.AppendLine("\t}");


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
                _generateList(
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
                    buffer.Append($"public {overrideString}IReadOnlyDictionary<string, {objectNode.GenericType}> {key} => ");

                    _generateMap(
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
                    config: config,
                    language: localeData.Locale.TwoLetterISOLanguageName,
                    node: pluralNod,
                    depth: 0
                );
            }
        }

        return buffer.ToString();
    }

    /// generates a class and all of its members of ONE locale
    /// adds subclasses to the queue
    private static string _generateClass(
        GenerateConfig config,
        I18NData localeData,
        Queue<ClassTask> queue,
        string className,
        ObjectNode node
    )
    {
        StringBuilder buffer = new();

        buffer.AppendLine();

        buffer.AppendLine($"// Path: {node.Path}");

        // The root class of **this** locale (path-independent).
        string rootClassName = localeData.BaseLocale
            ? config.ClassName
            : GetClassNameRoot(
                baseName: config.BaseName,
                locale: localeData.Locale
            );

        // The current class name.
        string varClassName = GetClassName(
            parentName: className,
            locale: localeData.Locale);

        if (localeData.BaseLocale)
        {
            buffer.AppendLine($"public class {varClassName}");
            buffer.AppendLine("{");
        }
        else
        {
            // The class name of the **base** locale (path-dependent).
            string baseClassName = GetClassName(
                parentName: className,
                locale: config.BaseLocale);

            if (config.FallbackStrategy == GenerateFallbackStrategy.None)
            {
                buffer.AppendLine(
                    $"public class {varClassName} : {baseClassName}");
                buffer.AppendLine("{");
            }
            else
            {
                buffer.AppendLine($"public class {varClassName} : {baseClassName}");
                buffer.AppendLine("{");
            }
        }

// constructor and custom fields
        bool callSuperConstructor = !localeData.BaseLocale &&
                                    config.FallbackStrategy == GenerateFallbackStrategy.BaseLocale;

        if (callSuperConstructor)
            buffer.AppendLine($"\tpublic {varClassName}({rootClassName} root) : base(root)");
        else
            buffer.AppendLine($"\tpublic {varClassName}({rootClassName} root)");

        AddTabs(buffer, 1);
        buffer.AppendLine("{");
        AddTabs(buffer, 2);
        buffer.AppendLine("this._root = root;");

        foreach ((string key, var value) in node.Entries)
        {
            if (value is ObjectNode objectNode)
            {
                if (!objectNode.IsMap)
                {
                    string childClassWithLocale = GetClassName(
                        parentName: className, childName: key, locale: localeData.Locale);

                    buffer.AppendLine(
                        $"\t\t{key} = new {childClassWithLocale}(_root);");
                }
            }
        }


        AddTabs(buffer, 1);
        buffer.AppendLine("}");


// root
        buffer.AppendLine();

        string overrideOrVirtual = !localeData.BaseLocale ? "override" : "virtual";

        buffer.Append($"\tprotected {overrideOrVirtual} {rootClassName} _root {{ get; }}");

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
                _generateList(
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
                    buffer.Append($"public {overrideString}IReadOnlyDictionary<string, {objectNode.GenericType}> {key} => ");

                    _generateMap(
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
                    config: config,
                    language: localeData.Locale.TwoLetterISOLanguageName,
                    node: pluralNod,
                    depth: 0
                );
            }
        }

        buffer.AppendLine("}");

        return buffer.ToString();
    }

    /// generates a map of ONE locale
    /// similar to _generateClass but anonymous and accessible via key
    private static void _generateMap(
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
                _generateList(
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
                    _generateMap(
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
                    config: config,
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
    private static void _generateList(
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
                _generateList(
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
                    _generateMap(
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
                    config: config,
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

    /// returns a map containing all parameters
    /// e.g. {"name": name, "age": age}
    private static string _toParameterMap(HashSet<string> @params)
    {
        StringBuilder buffer = new();
        buffer.Append('{');
        bool first = true;
        foreach (string param in @params)
        {
            if (!first) buffer.Append(", ");
            buffer.Append('"');
            buffer.Append(param);
            buffer.Append("\": ");
            buffer.Append(param);
            first = false;
        }

        buffer.Append('}');

        return buffer.ToString();
    }

    private static void AddPluralCall(
        StringBuilder buffer,
        GenerateConfig config,
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
            AddTabs(buffer, depth + 2);
            var textNode = quantity.Value as StringTextNode;

            buffer.Append(
                $"{quantity.Key.ParamName()}: ${GetStringLiteral(textNode.Content, textNode.Links.Count)}");

            if (count != node.Quantities.Count - 1)
                buffer.AppendLine(",");

            count++;
        }

        AddTabs(buffer, depth + 1);
        buffer.Append(')');

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