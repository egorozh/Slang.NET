using Slang;
using Slang.Showcase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Slang.Showcase
{
	class StringsRu : Strings
	{
		protected override StringsRu _root { get; } // ignore: unused_field

		public StringsRu()
		{
			_root = this;
			Core = new StringsCoreRu(_root);
			SomeKey = new StringsSomeKeyRu(_root);
			Alert = new StringsAlertRu(_root);
			Bookmarks = new StringsBookmarksRu(_root);
			ContextMenu = new StringsContextMenuRu(_root);
			ExplorerTab = new StringsExplorerTabRu(_root);
			FileOperations = new StringsFileOperationsRu(_root);
			Pages = new StringsPagesRu(_root);
			Tab = new StringsTabRu(_root);
			MyComputer = new StringsMyComputerRu(_root);
			Presenters = new StringsPresentersRu(_root);
			NotifyIcon = new StringsNotifyIconRu(_root);
			PageFactory = new StringsPageFactoryRu(_root);
			SearchHandler = new StringsSearchHandlerRu(_root);
			TabsControl = new StringsTabsControlRu(_root);
		}

	// Translations
	public override StringsCoreRu Core { get; }
	public override StringsSomeKeyRu SomeKey { get; }
	public override StringsAlertRu Alert { get; }
	public override StringsBookmarksRu Bookmarks { get; }
	public override StringsContextMenuRu ContextMenu { get; }
	public override StringsExplorerTabRu ExplorerTab { get; }
	public override StringsFileOperationsRu FileOperations { get; }
	public override StringsPagesRu Pages { get; }
	public override StringsTabRu Tab { get; }
	public override StringsMyComputerRu MyComputer { get; }
	public override StringsPresentersRu Presenters { get; }
	public override StringsNotifyIconRu NotifyIcon { get; }
	public override StringsPageFactoryRu PageFactory { get; }
	public override StringsSearchHandlerRu SearchHandler { get; }
	public override StringsTabsControlRu TabsControl { get; }

	// Path: Core
	public class StringsCoreRu : StringsCoreEn
	{
		public StringsCoreRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations

	/// Byte short
	public override string B => "Б";

	public override string Gb => "ГБ";
	public override string Kb => "КБ";
	public override string Mb => "МБ";
	public override string Tb => "ТБ";
	public override string Error => "Ошибка";
}

	// Path: SomeKey
	public class StringsSomeKeyRu : StringsSomeKeyEn
	{
		public StringsSomeKeyRu(StringsRu root) : base(root)
		{
			this._root = root;
			Fields = new StringsSomeKeyFieldsRu(_root);
			B = new StringsSomeKeyBRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Apple(int n) => PluralResolvers.Cardinal("ru")(n,
		one: $"У меня {n} яблоко.",
		few: $"У меня {n} яблока.",
		other: $"У меня {n} яблок."	);
	public override StringsSomeKeyFieldsRu Fields { get; }
	public override IReadOnlyDictionary<string, string> A => new Dictionary<string, string> {
		{"helloWorld", "привет"},
	};
	public override StringsSomeKeyBRu B { get; }
	public override List<dynamic> NiceList => [
		"привет",
		"блеск",
		new[]{
			"первый элемент в вложенном списке",
			"второй элемент в вложенном списке",
		},
		new StringsSomeKeyNiceList0i3Ru(_root),
		new StringsSomeKeyNiceList0i4Ru(_root),
	];
	public override string Introduce(string firstName, int age) => $"Привет, {_root.SomeKey.Fields.Name(firstName: firstName)} и {_root.SomeKey.Fields.Age(age: age)}";
}

	// Path: Alert
	public class StringsAlertRu : StringsAlertEn
	{
		public StringsAlertRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string OkButtonText => "Понятно";
}

	// Path: Bookmarks
	public class StringsBookmarksRu : StringsBookmarksEn
	{
		public StringsBookmarksRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string NewFolder => "Новая папка";
	public override string RootFolder => "Панель закладок";
}

	// Path: ContextMenu
	public class StringsContextMenuRu : StringsContextMenuEn
	{
		public StringsContextMenuRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string AddToBookmarks => "Добавить в закладки";
	public override string Copy => "Копировать";
	public override string Cut => "Вырезать";
	public override string Delete => "Удалить";
	public override string Open => "Открыть";
	public override string OpenInNativeExplorer => "Открыть в проводнике Windows";
	public override string OpenInNewSideTab => "Открыть в новой вкладке боковой панели";
	public override string OpenInNewTab => "Открыть в новой вкладке";
	public override string OpenInNewWindow => "Открыть в новом окне";
	public override string Paste => "Вставить";
	public override string Rename => "Переименовать";
}

	// Path: ExplorerTab
	public class StringsExplorerTabRu : StringsExplorerTabEn
	{
		public StringsExplorerTabRu(StringsRu root) : base(root)
		{
			this._root = root;
			ContextMenu = new StringsExplorerTabContextMenuRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override StringsExplorerTabContextMenuRu ContextMenu { get; }
}

	// Path: FileOperations
	public class StringsFileOperationsRu : StringsFileOperationsEn
	{
		public StringsFileOperationsRu(StringsRu root) : base(root)
		{
			this._root = root;
			FileAlreadyExistAlertView = new StringsFileOperationsFileAlreadyExistAlertViewRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string FileAlreadyExist(object file) => $"{file} уже существует";
	public override string OperationColumnName => "Операция";
	public override string ProgressColumnName => "Прогресс";
	public override string CopyAction => "копируется";
	public override string MoveAction => "перемещается";
	public override string FileOperationTitle(object source, object operation, object target) => $"Файл {source} {operation} в {target}";
	public override string FolderOperationTitle(object source, object operation, object target) => $"Папка {source} {operation} в {target}";
	public override StringsFileOperationsFileAlreadyExistAlertViewRu FileAlreadyExistAlertView { get; }
}

	// Path: Pages
	public class StringsPagesRu : StringsPagesEn
	{
		public StringsPagesRu(StringsRu root) : base(root)
		{
			this._root = root;
			Explorer = new StringsPagesExplorerRu(_root);
			Main = new StringsPagesMainRu(_root);
			NotFound = new StringsPagesNotFoundRu(_root);
			Settings = new StringsPagesSettingsRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override StringsPagesExplorerRu Explorer { get; }
	public override StringsPagesMainRu Main { get; }
	public override StringsPagesNotFoundRu NotFound { get; }
	public override StringsPagesSettingsRu Settings { get; }
}

	// Path: Tab
	public class StringsTabRu : StringsTabEn
	{
		public StringsTabRu(StringsRu root) : base(root)
		{
			this._root = root;
			SearchControl = new StringsTabSearchControlRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override StringsTabSearchControlRu SearchControl { get; }
}

	// Path: MyComputer
	public class StringsMyComputerRu : StringsMyComputerEn
	{
		public StringsMyComputerRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string DrivesHeader => "Диски";
	public override string FolderHeader => "Папки";
	public override string FreeFromText => "свободно из";
}

	// Path: Presenters
	public class StringsPresentersRu : StringsPresentersEn
	{
		public StringsPresentersRu(StringsRu root) : base(root)
		{
			this._root = root;
			Grouping = new StringsPresentersGroupingRu(_root);
			Sorting = new StringsPresentersSortingRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Content => "Содержимое";
	public override string LargeIcons => "Огромные значки";
	public override string RegularIcons => "Нормальные значки";
	public override string SmallIcons => "Маленькие значки";
	public override string Table => "Таблица";
	public override string TableDateOfChangeRow => "Дата изменения";
	public override string TableNameRow => "Имя";
	public override string TableSizeRow => "Размер";
	public override string TableTypeRow => "Тип";
	public override string Tiles => "Плитка";
	public override string DirectoryTypeName => "Папка с файлами";
	public override StringsPresentersGroupingRu Grouping { get; }
	public override StringsPresentersSortingRu Sorting { get; }
}

	// Path: NotifyIcon
	public class StringsNotifyIconRu : StringsNotifyIconEn
	{
		public StringsNotifyIconRu(StringsRu root) : base(root)
		{
			this._root = root;
			ContextMenu = new StringsNotifyIconContextMenuRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string ToolTipText => "Двойной клик для показа окна, правый клик - открыть меню";
	public override StringsNotifyIconContextMenuRu ContextMenu { get; }
}

	// Path: PageFactory
	public class StringsPageFactoryRu : StringsPageFactoryEn
	{
		public StringsPageFactoryRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string NotAccessCaption => "\"Нет доступа\"";
	public override string NotAccessText => "У вас нет доступа к папке";
}

	// Path: SearchHandler
	public class StringsSearchHandlerRu : StringsSearchHandlerEn
	{
		public StringsSearchHandlerRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string RouteText(object route) => $"Перейти в \"{route}\"";
	public override string SearchAllDrivesText(object query) => $"Поиск \"{query}\" по всем папкам";
	public override string SearchDirectoryText(object query, object directory) => $"Поиск \"{query}\" в \"{directory}\"";
	public override string SearchDriveText(object query, object drive) => $"Поиск \"{query}\" в \"{drive}\"";
}

	// Path: TabsControl
	public class StringsTabsControlRu : StringsTabsControlEn
	{
		public StringsTabsControlRu(StringsRu root) : base(root)
		{
			this._root = root;
			ContextMenu = new StringsTabsControlContextMenuRu(_root);
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override StringsTabsControlContextMenuRu ContextMenu { get; }
}

	// Path: SomeKey.Fields
	public class StringsSomeKeyFieldsRu : StringsSomeKeyFieldsEn
	{
		public StringsSomeKeyFieldsRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Name(string firstName) => $"моё имя {firstName}";
	public override string Age(int age) => $"Мне {age} лет";
	public override string Date(DateOnly date) => $"Дата моего дня рождения: {date}";
}

	// Path: SomeKey.B
	public class StringsSomeKeyBRu : StringsSomeKeyBEn
	{
		public StringsSomeKeyBRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string B0 => "Хай";
	public override IReadOnlyDictionary<string, string> B1 => new Dictionary<string, string> {
		{"hiThere", "хай"},
	};
}

	// Path: SomeKey.NiceList.3
	public class StringsSomeKeyNiceList0i3Ru : StringsSomeKeyNiceList0i3En
	{
		public StringsSomeKeyNiceList0i3Ru(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Wow => "WOW!";
	public override string Ok => "OK!";
}

	// Path: SomeKey.NiceList.4
	public class StringsSomeKeyNiceList0i4Ru : StringsSomeKeyNiceList0i4En
	{
		public StringsSomeKeyNiceList0i4Ru(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string AMapEntry => "доступ через ключ";
	public override string AnotherEntry => "доступ через второй ключ";
}

	// Path: ExplorerTab.ContextMenu
	public class StringsExplorerTabContextMenuRu : StringsExplorerTabContextMenuEn
	{
		public StringsExplorerTabContextMenuRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string OpenInNativeExplorer => "Открыть в проводнике Windows";
	public override string BackButtonGesture => "Alt + Стрелка влево";
	public override string BackButtonHeader => "Назад";
	public override string Create => "Создать";
	public override string CreateFolder => "Папку";
	public override string CreateTextFile => "Текстовый документ";
	public override string ForwardButtonGesture => "Alt + Стрелка вправо";
	public override string ForwardButtonHeader => "Вперёд";
	public override string Grouping => "Группировка";
	public override string Paste => "Вставить";
	public override string Properties => "Свойства";
	public override string Sorting => "Сортировка";
	public override string UpdateButtonHeader => "Обновить";
	public override string View => "Вид";
}

	// Path: FileOperations.FileAlreadyExistAlertView
	public class StringsFileOperationsFileAlreadyExistAlertViewRu : StringsFileOperationsFileAlreadyExistAlertViewEn
	{
		public StringsFileOperationsFileAlreadyExistAlertViewRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Title => "Предупреждение";
	public override string ReplaceButtonText => "Заменить";
	public override string SaveBothButtonText => "Сохранить оба";
	public override string SkipButtonText => "Пропустить";
	public override string CancelButtonText => "Отмена";
	public override string AcceptAllFiles => "Применить для оставшихся файлов";
}

	// Path: Pages.Explorer
	public class StringsPagesExplorerRu : StringsPagesExplorerEn
	{
		public StringsPagesExplorerRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string NewFolder => "Новая папка";
	public override string NewTextFile => "Новый текстовый документ";
	public override string GridNameColumnHeader => "Имя";
	public override string GridChangedAtColumnHeader => "Дата изменения";
	public override string SelectedItems(object count) => $"Выбрано: {count}";
	public override string FolderEmpty => "Папка пустая";
	public override string NotAccess => "Нет доступа";
}

	// Path: Pages.Main
	public class StringsPagesMainRu : StringsPagesMainEn
	{
		public StringsPagesMainRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Title => "Главная";
}

	// Path: Pages.NotFound
	public class StringsPagesNotFoundRu : StringsPagesNotFoundEn
	{
		public StringsPagesNotFoundRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string PageNotFound => "Страница не найдена";
}

	// Path: Pages.Settings
	public class StringsPagesSettingsRu : StringsPagesSettingsEn
	{
		public StringsPagesSettingsRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Title => "Настройки";
	public override string GeneralTabLabel => "Общие";
	public override string ExplorerTabLabel => "Папки и файлы";
	public override string YandexDiskTabLabel => "Yandex Disk";
	public override string PresenterGridType => "Таблица";
	public override string PresenterIconsType => "Плитка";
	public override string ThemeLightVariant => "Светлая";
	public override string ThemeDarkVariant => "Темная";
	public override string StartLayoutLastMode => "Запоминать последнюю конфигурацию";
	public override string StartLayoutOnePanelMode => "Однопанельный режим";
	public override string StartLayoutTwoPanelMode => "Двухпанельный режим";
	public override string ShowHiddenItemsLabel => "Показывать скрытые папки и файлы";
	public override string ShowSystemItemsLabel => "Показывать системные папки и файлы";
	public override string PresentationLabel => "Представление";
	public override string ThemeLabel => "Тема";
	public override string LayoutModeLabel => "Кол-во панелей";
	public override string YandexDiskTitle => "Yandex Disk";
	public override string YandexDiskAuthTitle => "Авторизация:";
	public override string GetCodeLabel => "Получить код";
	public override string EnterCodeLabel => "Ввести код";
	public override string YandexSuccessAuthoriseLabel => "Вы авторизованы";
}

	// Path: Tab.SearchControl
	public class StringsTabSearchControlRu : StringsTabSearchControlEn
	{
		public StringsTabSearchControlRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string AddBookmark => "Добавить закладку";
	public override string NameLabel => "Имя: ";
	public override string FolderLabel => "Папка: ";
	public override string PathLabel => "Путь: ";
	public override string AdditionalButtonText => "Дополнительно";
	public override string DoneButtonText => "Готово";
	public override string RemoveButtonText => "Удалить";
	public override string ChangeFavorites => "Изменить избранное";
	public override string NewFolderButtonText => "Новая папка";
	public override string SaveButtonText => "Сохранить";
	public override string CancelButtonText => "Отмена";
}

	// Path: Presenters.Grouping
	public class StringsPresentersGroupingRu : StringsPresentersGroupingEn
	{
		public StringsPresentersGroupingRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Name => "Имя";
	public override string None => "Нет";
	public override string Type => "Тип";
}

	// Path: Presenters.Sorting
	public class StringsPresentersSortingRu : StringsPresentersSortingEn
	{
		public StringsPresentersSortingRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string DateOfChange => "Дата изменения";
	public override string Name => "Имя";
	public override string Size => "Размер";
	public override string Type => "Тип";
}

	// Path: NotifyIcon.ContextMenu
	public class StringsNotifyIconContextMenuRu : StringsNotifyIconContextMenuEn
	{
		public StringsNotifyIconContextMenuRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string ExitHeader => "Выйти";
	public override string ShowWindowHeader => "Показать окно";
}

	// Path: TabsControl.ContextMenu
	public class StringsTabsControlContextMenuRu : StringsTabsControlContextMenuEn
	{
		public StringsTabsControlContextMenuRu(StringsRu root) : base(root)
		{
			this._root = root;
		}

		protected override StringsRu _root { get; } // ignore: unused_field

	// Translations
	public override string Close => "Закрыть";
	public override string CloseOtherTabs => "Закрыть другие вкладки";
	public override string Duplicate => "Дублировать";
	public override string NewTabOnRight => "Новая вкладка справа";
	public override string OpenTabInNewWindow => "Открыть вкладку в новом окне";
	public override string Refresh => "Обновить";
}

   }
}