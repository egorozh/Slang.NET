using System.Globalization;
using Avalonia.Data.Converters;

namespace Slang.Desktop.Avalonia;

public static class Converters
{
    public class FileConverter: IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values is [Strings root, string filePath] ? root.File(filePath) : null;
        }
    }
    
    public static FileConverter File => new();

    public class SelectedFilesConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values is [Strings root, int selectedFilesCount] ? root.SelectedFiles(selectedFilesCount) : null;
        }
    }
    
    public static SelectedFilesConverter SelectedFiles => new();
    
    public class PriceConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values is [Strings root, decimal price] ? root.Price(price) : null;
        }
    }
    
    public static PriceConverter Price => new();
}

