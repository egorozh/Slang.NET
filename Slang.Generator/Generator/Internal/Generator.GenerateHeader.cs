using System.Text;
using Slang.Generator.Generator.Entities;
using Slang.Generator.Nodes.Nodes;
using Slang.Generator.NodesData;
using static Slang.Generator.Generator.Helper;

namespace Slang.Generator.Generator;

internal static partial class Generator
{
    private static string GenerateHeader(GenerateConfig config, List<I18NData> allLocales)
    {
        return
            $$"""
              {{GenerateHeaderComment(
                  translations: allLocales,
                  now: DateTime.Now.ToLocalTime()
              )}}

              {{GenerateImports(config.Namespace)}}

              namespace {{config.Namespace}}
              {
              	partial class {{config.ClassName}}
              	{
              {{GenerateCultures(config, allLocales)}}
              
                      private readonly static CultureInfo _baseCulture = _{{config.BaseLocale.TwoLetterISOLanguageName}};
              
              		public static IReadOnlyList<CultureInfo> SupportedCultures => _translations.Keys.ToList();
              
              		public static Strings Translations
              		{
              			get
              			{
              				var culture = CultureInfo.CurrentUICulture;
              
              				if (_translations.TryGetValue(culture, out var translation))
              					return translation;
              					
                            var sameCulture = SupportedCultures.FirstOrDefault(c => c.TwoLetterISOLanguageName == culture.TwoLetterISOLanguageName);
              
                            if (sameCulture != null)
                                return _translations[sameCulture];
                                
              				return _translations[_baseCulture];
              			}
              		}
              
              		public static void SetCulture(CultureInfo culture, bool uiOnly = false)
              		{
              			if (!uiOnly)
              				Thread.CurrentThread.CurrentCulture = culture;
              
              			Thread.CurrentThread.CurrentUICulture = culture;
              		}
              	}
              }
              """;
    }

    private static string GenerateHeaderComment(List<I18NData> translations, DateTime now)
    {
        StringBuilder buffer = new();

        int count = translations.Aggregate(
            0,
            (prev, curr) => prev + CountTranslations(curr.Root)
        );

        string countPerLocale = "";

        if (translations.Count != 1)
            countPerLocale = $" ({Math.Floor((double) count / translations.Count)} per locale)";

        string statisticsStr =
            $"""
             ///
             /// Locales: {translations.Count}
             /// Strings: {count}{countPerLocale}
             """;

        string date = $"{now.Year}-{TwoDigits(now.Month)}-{TwoDigits(now.Day)}";
        string time = $"{TwoDigits(now.Hour)}:{TwoDigits(now.Minute)}";
        string timestampStr = $"""
                               ///
                               /// Built on {date} at {time} UTC
                               """;


        buffer.AppendLine($"""
                           /// Generated file. Do not edit.
                           ///
                           {statisticsStr}{timestampStr}
                           """);
        
        return buffer.ToString();

        string TwoDigits(int value) => value.ToString().PadLeft(2, '0');
    }

    private static string GenerateImports(string @namespace)
    {
        StringBuilder buffer = new();

        List<string> imports =
        [
            "Slang",
            "System.Collections.Generic",
            "System.Globalization",
            "System.Linq",
            "System",
            "System.Threading",
            @namespace
        ];

        imports = imports.Order().ToList();

        foreach (string i in imports)
            buffer.AppendLine($"using {i};");

        return buffer.ToString();
    }

    private static string GenerateCultures(GenerateConfig config, List<I18NData> allLocales)
    {
        StringBuilder buffer = new();

        for (int i = 0; i < allLocales.Count; i++)
        {
            var locale = allLocales[i].Locale;

            buffer.AppendLine(
                $"\t\tprivate readonly static CultureInfo _{locale.TwoLetterISOLanguageName} = new CultureInfo(\"{locale.Name}\");");
        }

        buffer.AppendLine();

        buffer.AppendLine(
            """
                    private readonly static IReadOnlyDictionary<CultureInfo, Strings> _translations =
                        new Dictionary<CultureInfo, Strings>()
                        {
            """);

        for (int i = 0; i < allLocales.Count; i++)
        {
            var locale = allLocales[i].Locale;

            string className = allLocales[i].BaseLocale
                ? config.ClassName
                : GetClassNameRoot(
                    baseName: config.BaseName,
                    locale: locale
                );

            buffer.Append(
                $"\t\t\t\t{{_{locale.TwoLetterISOLanguageName}, new {className}() }}");

            if (i < allLocales.Count - 1)
                buffer.Append(',');

            buffer.AppendLine();
        }


        buffer.AppendLine("            };");

        return buffer.ToString();
    }

    private static int CountTranslations(Node node) => node switch
    {
        StringTextNode => 1,
        ListNode listNode => listNode.Entries.Sum(CountTranslations),
        ObjectNode objectNode => objectNode.Entries.Values.Sum(CountTranslations),
        PluralNode pluralNode => pluralNode.Quantities.Count,
        _ => 0
    };
}