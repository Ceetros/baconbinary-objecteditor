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
                int targetIndex = 0;
                bool inverse = false;

                if (parameter is string paramStr)
                {
                    if (paramStr.StartsWith("!"))
                    {
                        inverse = true;
                        paramStr = paramStr.Substring(1);
                    }

                    if (int.TryParse(paramStr, out int parsedIndex))
                    {
                        targetIndex = parsedIndex;
                    }
                }

                bool match = categoryIndex == targetIndex;
                return inverse ? !match : match;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
