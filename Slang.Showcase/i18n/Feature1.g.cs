/// Generated file. Do not edit.
///
///
/// Locales: 2
///
/// Built on 25.09.2024 at 19:41 UTC

using Slang;
using Slang.Showcase.MyNamespace;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Slang.Showcase.MyNamespace
{
	partial class Feature1
	{
		private readonly static CultureInfo _en = new CultureInfo("en");
		private readonly static CultureInfo _ru = new CultureInfo("ru");

        private readonly static IReadOnlyDictionary<CultureInfo, Feature1> _translations =
            new Dictionary<CultureInfo, Feature1>()
            {
				{_en, new Feature1() },
				{_ru, new Feature1Ru() }
            };


        private readonly static CultureInfo _baseCulture = _en;

		public static IReadOnlyList<CultureInfo> SupportedCultures => _translations.Keys.ToList();

		public static Feature1 Translations
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