using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BaconBinary.ObjectEditor.UI.Converters
{
    public class CategoryToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int categoryIndex)
            {
                
                bool isItem = categoryIndex == 0;
                
                if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
                {
                    return !isItem;
                }

                return isItem;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
