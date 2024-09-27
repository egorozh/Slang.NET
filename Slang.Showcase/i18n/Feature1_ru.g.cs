using Slang;
using Slang.Showcase.MyNamespace;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Slang.Showcase.MyNamespace
{
	partial class Feature1Ru : Feature1
	{
		protected override Feature1Ru _root { get; } // ignore: unused_field

		public Feature1Ru()
		{
			_root = this;
			Screen = new Feature1ScreenRu(_root);
		}

		// Translations
		public override Feature1ScreenRu Screen { get; }

		// Path: Screen
		public class Feature1ScreenRu : Feature1ScreenEn
		{
			public Feature1ScreenRu(Feature1Ru root) : base(root)
			{
				this._root = root;
			}

			protected override Feature1Ru _root { get; } // ignore: unused_field

			// Translations
			public override string Locale1 => "Локаль 1";
		}

   }
}