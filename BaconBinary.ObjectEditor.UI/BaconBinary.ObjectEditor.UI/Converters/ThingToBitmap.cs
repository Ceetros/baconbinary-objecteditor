using System;
using System.Globalization;
using Avalonia.Data.Converters;
using BaconBinary.Core.Models;
using BaconBinary.ObjectEditor.UI.Services;

namespace BaconBinary.ObjectEditor.UI.Converters
{
    public class ThingToBitmapConverter : IValueConverter
    {
        public static SpriteProvider Provider { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThingType thing && Provider != null)
            {
                return Provider.GetThingBitmap(thing);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}