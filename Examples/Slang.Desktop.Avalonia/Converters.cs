using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Slang.Desktop.Avalonia;

public static class Converters
{
    public class FileConverter: IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is [Strings root, string filePath])
            {
                return root.File(filePath);
            }
        
            return null;
        }
    }
    
    public static FileConverter File => new();

    public class SelectedFilesConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is [Strings root, int selectedFilesCount])
            {
                return root.SelectedFiles(selectedFilesCount);
            }

            return null;
        }
    }
    
    public static SelectedFilesConverter SelectedFiles => new();
    
    public class PriceConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values is [Strings root, decimal price])
            {
                return root.Price(price);
            }

            return null;
        }
    }
    
    public static PriceConverter Price => new();
}

