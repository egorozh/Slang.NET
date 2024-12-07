using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Slang.Desktop.Avalonia;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string _filePath = @"C:\\Users\\Egorozh\\AppData\\Local\\Temp\\SourceGeneratedDocuments\\DDF33890A3A44C94C25B4DB9\\Slang.Generator\\Slang.Generator.TranslateGenerator\\Strings_ru.g.cs";

    [ObservableProperty] private int _selectedFilesCount;
    
    [ObservableProperty] private decimal _price;
    
    [ObservableProperty] private CultureInfo _selectedCulture = Strings.BaseCulture;

    partial void OnSelectedCultureChanged(CultureInfo value)
    {
        Strings.SetCulture(value);
        //todo: save culture to settings
    }
}