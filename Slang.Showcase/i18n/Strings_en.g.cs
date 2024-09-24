using Slang;
using Slang.Showcase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Slang.Showcase;

partial class Strings
{


	protected virtual Strings _root { get; } // ignore: unused_field

	public Strings()
	{
		_root = this;
		Core = new StringsCoreEn(_root);
		SomeKey = new StringsSomeKeyEn(_root);
		Alert = new StringsAlertEn(_root);
		Bookmarks = new StringsBookmarksEn(_root);
		ContextMenu = new StringsContextMenuEn(_root);
		ExplorerTab = new StringsExplorerTabEn(_root);
		FileOperations = new StringsFileOperationsEn(_root);
		Pages = new StringsPagesEn(_root);
		Tab = new StringsTabEn(_root);
		MyComputer = new StringsMyComputerEn(_root);
		Presenters = new StringsPresentersEn(_root);
		NotifyIcon = new StringsNotifyIconEn(_root);
		PageFactory = new StringsPageFactoryEn(_root);
		SearchHandler = new StringsSearchHandlerEn(_root);
		TabsControl = new StringsTabsControlEn(_root);
	}

	// Translations
	public virtual StringsCoreEn Core { get; }
	public virtual StringsSomeKeyEn SomeKey { get; }
	public virtual StringsAlertEn Alert { get; }
	public virtual StringsBookmarksEn Bookmarks { get; }
	public virtual StringsContextMenuEn ContextMenu { get; }
	public virtual StringsExplorerTabEn ExplorerTab { get; }
	public virtual StringsFileOperationsEn FileOperations { get; }
	public virtual StringsPagesEn Pages { get; }
	public virtual StringsTabEn Tab { get; }
	public virtual StringsMyComputerEn MyComputer { get; }
	public virtual StringsPresentersEn Presenters { get; }
	public virtual StringsNotifyIconEn NotifyIcon { get; }
	public virtual StringsPageFactoryEn PageFactory { get; }
	public virtual StringsSearchHandlerEn SearchHandler { get; }
	public virtual StringsTabsControlEn TabsControl { get; }

	
	// Path: Core
	public class StringsCoreEn
	{
		public StringsCoreEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string B => "B";
		public virtual string Gb => "GB";
		public virtual string Kb => "KB";
		public virtual string Mb => "MB";
		public virtual string Tb => "TB";
		public virtual string Error => "Error";
	}
	
	
	// Path: SomeKey
	public class StringsSomeKeyEn
	{
		public StringsSomeKeyEn(Strings root)
		{
			this._root = root;
			Fields = new StringsSomeKeyFieldsEn(_root);
			B = new StringsSomeKeyBEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Apple(int n) => PluralResolvers.Cardinal("en")(n,
			one: $"I have {n} apple.",
			few: $"I have {n} apples",
			other: $"I have {n} apples."	);
		public virtual StringsSomeKeyFieldsEn Fields { get; }
		public virtual IReadOnlyDictionary<string, string> A => new Dictionary<string, string> {
			{"helloWorld", "hello"},
		};
		public virtual StringsSomeKeyBEn B { get; }
		public virtual List<dynamic> NiceList => [
			"hello",
			"nice",
			new[]{
				"first item in nested list",
				"second item in nested list",
			},
			new StringsSomeKeyNiceList0i3En(_root),
			new StringsSomeKeyNiceList0i4En(_root),
		];
		public virtual string Introduce(string firstName, int age) => $"Hello, {_root.SomeKey.Fields.Name(firstName: firstName)} and {_root.SomeKey.Fields.Age(age: age)}";
	}
	
	
	// Path: Alert
	public class StringsAlertEn
	{
		public StringsAlertEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string OkButtonText => "Understood";
	}
	
	
	// Path: Bookmarks
	public class StringsBookmarksEn
	{
		public StringsBookmarksEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string NewFolder => "New Folder";
		public virtual string RootFolder => "Bookmarks Panel";
	}
	
	
	// Path: ContextMenu
	public class StringsContextMenuEn
	{
		public StringsContextMenuEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string AddToBookmarks => "Add to bookmarks";
		public virtual string Copy => "Copy";
		public virtual string Cut => "Cut";
		public virtual string Delete => "Delete";
		public virtual string Open => "Open";
		public virtual string OpenInNativeExplorer => "Open in Windows Explorer";
		public virtual string OpenInNewSideTab => "Open in new side tab";
		public virtual string OpenInNewTab => "Open in new tab";
		public virtual string OpenInNewWindow => "Open in new window";
		public virtual string Paste => "Paste";
		public virtual string Rename => "Rename";
	}
	
	
	// Path: ExplorerTab
	public class StringsExplorerTabEn
	{
		public StringsExplorerTabEn(Strings root)
		{
			this._root = root;
			ContextMenu = new StringsExplorerTabContextMenuEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual StringsExplorerTabContextMenuEn ContextMenu { get; }
	}
	
	
	// Path: FileOperations
	public class StringsFileOperationsEn
	{
		public StringsFileOperationsEn(Strings root)
		{
			this._root = root;
			FileAlreadyExistAlertView = new StringsFileOperationsFileAlreadyExistAlertViewEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string FileAlreadyExist(object file) => $"{file} already exists";
		public virtual string OperationColumnName => "Operation";
		public virtual string ProgressColumnName => "Progress";
		public virtual string CopyAction => "is being copied";
		public virtual string MoveAction => "is being moved";
		public virtual string FileOperationTitle(object source, object operation, object target) => $"File {source} {operation} to {target}";
		public virtual string FolderOperationTitle(object source, object operation, object target) => $"Folder {source} {operation} to {target}";
		public virtual StringsFileOperationsFileAlreadyExistAlertViewEn FileAlreadyExistAlertView { get; }
	}
	
	
	// Path: Pages
	public class StringsPagesEn
	{
		public StringsPagesEn(Strings root)
		{
			this._root = root;
			Explorer = new StringsPagesExplorerEn(_root);
			Main = new StringsPagesMainEn(_root);
			NotFound = new StringsPagesNotFoundEn(_root);
			Settings = new StringsPagesSettingsEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual StringsPagesExplorerEn Explorer { get; }
		public virtual StringsPagesMainEn Main { get; }
		public virtual StringsPagesNotFoundEn NotFound { get; }
		public virtual StringsPagesSettingsEn Settings { get; }
	}
	
	
	// Path: Tab
	public class StringsTabEn
	{
		public StringsTabEn(Strings root)
		{
			this._root = root;
			SearchControl = new StringsTabSearchControlEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual StringsTabSearchControlEn SearchControl { get; }
	}
	
	
	// Path: MyComputer
	public class StringsMyComputerEn
	{
		public StringsMyComputerEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string DrivesHeader => "Drives";
		public virtual string FolderHeader => "Folders";
		public virtual string FreeFromText => "free from";
	}
	
	
	// Path: Presenters
	public class StringsPresentersEn
	{
		public StringsPresentersEn(Strings root)
		{
			this._root = root;
			Grouping = new StringsPresentersGroupingEn(_root);
			Sorting = new StringsPresentersSortingEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Content => "Content";
		public virtual string LargeIcons => "Large icons";
		public virtual string RegularIcons => "Regular icons";
		public virtual string SmallIcons => "Small icons";
		public virtual string Table => "Table";
		public virtual string TableDateOfChangeRow => "Date of change";
		public virtual string TableNameRow => "Name";
		public virtual string TableSizeRow => "Size";
		public virtual string TableTypeRow => "Type";
		public virtual string Tiles => "Tiles";
		public virtual string DirectoryTypeName => "Folder with files";
		public virtual StringsPresentersGroupingEn Grouping { get; }
		public virtual StringsPresentersSortingEn Sorting { get; }
	}
	
	
	// Path: NotifyIcon
	public class StringsNotifyIconEn
	{
		public StringsNotifyIconEn(Strings root)
		{
			this._root = root;
			ContextMenu = new StringsNotifyIconContextMenuEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string ToolTipText => "Double click to show the window, right click - open menu";
		public virtual StringsNotifyIconContextMenuEn ContextMenu { get; }
	}
	
	
	// Path: PageFactory
	public class StringsPageFactoryEn
	{
		public StringsPageFactoryEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string NotAccessCaption => "\"No access\"";
		public virtual string NotAccessText => "You do not have access to the folder";
	}
	
	
	// Path: SearchHandler
	public class StringsSearchHandlerEn
	{
		public StringsSearchHandlerEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string RouteText(object route) => $"Go to \"{route}\"";
		public virtual string SearchAllDrivesText(object query) => $"Search \"{query}\" in all folders";
		public virtual string SearchDirectoryText(object query, object directory) => $"Search \"{query}\" in \"{directory}\"";
		public virtual string SearchDriveText(object query, object drive) => $"Search \"{query}\" in \"{drive}\"";
	}
	
	
	// Path: TabsControl
	public class StringsTabsControlEn
	{
		public StringsTabsControlEn(Strings root)
		{
			this._root = root;
			ContextMenu = new StringsTabsControlContextMenuEn(_root);
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual StringsTabsControlContextMenuEn ContextMenu { get; }
	}
	
	
	// Path: SomeKey.Fields
	public class StringsSomeKeyFieldsEn
	{
		public StringsSomeKeyFieldsEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Name(string firstName) => $"my name is {firstName}";
		public virtual string Age(int age) => $"I am {age} years old";
		public virtual string Date(DateOnly date) => $"My birthday date: {date}";
	}
	
	
	// Path: SomeKey.B
	public class StringsSomeKeyBEn
	{
		public StringsSomeKeyBEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string B0 => "hey";
		public virtual IReadOnlyDictionary<string, string> B1 => new Dictionary<string, string> {
			{"hiThere", "hi"},
		};
	}
	
	
	// Path: SomeKey.NiceList.3
	public class StringsSomeKeyNiceList0i3En
	{
		public StringsSomeKeyNiceList0i3En(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Wow => "WOW!";
		public virtual string Ok => "OK!";
	}
	
	
	// Path: SomeKey.NiceList.4
	public class StringsSomeKeyNiceList0i4En
	{
		public StringsSomeKeyNiceList0i4En(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string AMapEntry => "access via key";
		public virtual string AnotherEntry => "access via second key";
	}
	
	
	// Path: ExplorerTab.ContextMenu
	public class StringsExplorerTabContextMenuEn
	{
		public StringsExplorerTabContextMenuEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string OpenInNativeExplorer => "Open in Windows Explorer";
		public virtual string BackButtonGesture => "Alt + Left Arrow";
		public virtual string BackButtonHeader => "Back";
		public virtual string Create => "Create";
		public virtual string CreateFolder => "Folder";
		public virtual string CreateTextFile => "Text Document";
		public virtual string ForwardButtonGesture => "Alt + Right Arrow";
		public virtual string ForwardButtonHeader => "Forward";
		public virtual string Grouping => "Grouping";
		public virtual string Paste => "Paste";
		public virtual string Properties => "Properties";
		public virtual string Sorting => "Sorting";
		public virtual string UpdateButtonHeader => "Update";
		public virtual string View => "View";
	}
	
	
	// Path: FileOperations.FileAlreadyExistAlertView
	public class StringsFileOperationsFileAlreadyExistAlertViewEn
	{
		public StringsFileOperationsFileAlreadyExistAlertViewEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Title => "Warning";
		public virtual string ReplaceButtonText => "Replace";
		public virtual string SaveBothButtonText => "Save Both";
		public virtual string SkipButtonText => "Skip";
		public virtual string CancelButtonText => "Cancel";
		public virtual string AcceptAllFiles => "Apply to remaining files";
	}
	
	
	// Path: Pages.Explorer
	public class StringsPagesExplorerEn
	{
		public StringsPagesExplorerEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string NewFolder => "New Folder";
		public virtual string NewTextFile => "New Text Document";
		public virtual string GridNameColumnHeader => "Name";
		public virtual string GridChangedAtColumnHeader => "Date Modified";
		public virtual string SelectedItems(object count) => $"Selected: {count}";
		public virtual string FolderEmpty => "Folder is empty";
		public virtual string NotAccess => "No access";
	}
	
	
	// Path: Pages.Main
	public class StringsPagesMainEn
	{
		public StringsPagesMainEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Title => "Home";
	}
	
	
	// Path: Pages.NotFound
	public class StringsPagesNotFoundEn
	{
		public StringsPagesNotFoundEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string PageNotFound => "Page not found";
	}
	
	
	// Path: Pages.Settings
	public class StringsPagesSettingsEn
	{
		public StringsPagesSettingsEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Title => "Settings";
		public virtual string GeneralTabLabel => "General";
		public virtual string ExplorerTabLabel => "Folders and Files";
		public virtual string YandexDiskTabLabel => "Yandex Disk";
		public virtual string PresenterGridType => "Table";
		public virtual string PresenterIconsType => "Tile";
		public virtual string ThemeLightVariant => "Light";
		public virtual string ThemeDarkVariant => "Dark";
		public virtual string StartLayoutLastMode => "Remember last configuration";
		public virtual string StartLayoutOnePanelMode => "Single-panel mode";
		public virtual string StartLayoutTwoPanelMode => "Double-panel mode";
		public virtual string ShowHiddenItemsLabel => "Show Hidden Folders and Files";
		public virtual string ShowSystemItemsLabel => "Show System Folders and Files";
		public virtual string PresentationLabel => "Presentation";
		public virtual string ThemeLabel => "Theme";
		public virtual string LayoutModeLabel => "Number of Panels";
		public virtual string YandexDiskTitle => "Yandex Disk";
		public virtual string YandexDiskAuthTitle => "Authorization:";
		public virtual string GetCodeLabel => "Get Code";
		public virtual string EnterCodeLabel => "Enter Code";
		public virtual string YandexSuccessAuthoriseLabel => "You are authorized";
	}
	
	
	// Path: Tab.SearchControl
	public class StringsTabSearchControlEn
	{
		public StringsTabSearchControlEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string AddBookmark => "Add Bookmark";
		public virtual string NameLabel => "Name: ";
		public virtual string FolderLabel => "Folder: ";
		public virtual string PathLabel => "Path: ";
		public virtual string AdditionalButtonText => "More";
		public virtual string DoneButtonText => "Done";
		public virtual string RemoveButtonText => "Remove";
		public virtual string ChangeFavorites => "Change Favorites";
		public virtual string NewFolderButtonText => "New Folder";
		public virtual string SaveButtonText => "Save";
		public virtual string CancelButtonText => "Cancel";
	}
	
	
	// Path: Presenters.Grouping
	public class StringsPresentersGroupingEn
	{
		public StringsPresentersGroupingEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Name => "Name";
		public virtual string None => "None";
		public virtual string Type => "Type";
	}
	
	
	// Path: Presenters.Sorting
	public class StringsPresentersSortingEn
	{
		public StringsPresentersSortingEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string DateOfChange => "Date of change";
		public virtual string Name => "Name";
		public virtual string Size => "Size";
		public virtual string Type => "Type";
	}
	
	
	// Path: NotifyIcon.ContextMenu
	public class StringsNotifyIconContextMenuEn
	{
		public StringsNotifyIconContextMenuEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string ExitHeader => "Exit";
		public virtual string ShowWindowHeader => "Show window";
	}
	
	
	// Path: TabsControl.ContextMenu
	public class StringsTabsControlContextMenuEn
	{
		public StringsTabsControlContextMenuEn(Strings root)
		{
			this._root = root;
		}
	
		protected virtual Strings _root { get; } // ignore: unused_field
	
		// Translations
		public virtual string Close => "Close";
		public virtual string CloseOtherTabs => "Close other tabs";
		public virtual string Duplicate => "Duplicate";
		public virtual string NewTabOnRight => "New tab to the right";
		public virtual string OpenTabInNewWindow => "Open tab in new window";
		public virtual string Refresh => "Refresh";
	}
	
}
