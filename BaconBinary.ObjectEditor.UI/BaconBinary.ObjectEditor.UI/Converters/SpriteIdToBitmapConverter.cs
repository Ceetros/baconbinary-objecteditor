using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using BaconBinary.ObjectEditor.UI.Services;

namespace BaconBinary.ObjectEditor.UI.Converters
{
    public class SpriteIdToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint spriteId && ThingToBitmapConverter.Provider != null)
            {
                return ThingToBitmapConverter.Provider.GetSpriteBitmap(spriteId);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
