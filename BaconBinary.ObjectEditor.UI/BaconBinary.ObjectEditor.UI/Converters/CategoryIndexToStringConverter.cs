using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BaconBinary.ObjectEditor.UI.Converters
{
    public class CategoryIndexToStringConverter : IValueConverter
    {
        public static readonly CategoryIndexToStringConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index switch
                {
                    0 => "ITEMS",
                    1 => "OUTFITS",
                    2 => "EFFECTS",
                    3 => "MISSILES",
                    _ => "UNKNOWN"
                };
            }
            return "CATEGORY";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
