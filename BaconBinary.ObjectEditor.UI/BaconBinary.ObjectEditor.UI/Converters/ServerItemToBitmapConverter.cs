using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using BaconBinary.Core.Models;
using BaconBinary.ObjectEditor.UI.Services;

namespace BaconBinary.ObjectEditor.UI.Converters
{
    public class ServerItemToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ServerItem item && ThingToBitmapConverter.Provider != null)
            {
                // We need to find the ThingType corresponding to the ClientId.
                // Since we don't have direct access to the DatFile here, we might need a way to get it.
                // However, ThingToBitmapConverter uses a static Provider which is a SpriteProvider.
                // The SpriteProvider gets sprites by ID.
                // We need the ThingType to know which sprites to get.
                
                // A better approach might be to expose a method in a service to get the Bitmap for a ClientId.
                // For now, let's try to use the existing infrastructure.
                
                // If we assume the MainViewModel has the DatFile loaded, we could try to access it via a static property or service locator.
                // But that's messy.
                
                // Let's look at how ThingToBitmapConverter works.
                // It takes a ThingType.
                
                // If we can get the ThingType from the ClientId, we can reuse the logic.
                // The MainViewModel has the loaded DatFile.
                
                // Let's try to access the MainViewModel instance if possible, or pass the DatFile to the ItemEditorViewModel.
                
                // For this specific task, let's assume we can get the image if we have the ThingType.
                // But we only have ServerItem here.
                
                // Let's modify ItemEditorViewModel to hold a reference to the DatFile or a lookup method.
                // But first, let's see if we can just use the ClientId to get a sprite.
                // The ClientId usually corresponds to the ItemId in the Dat file.
                
                // If we look at MainViewModel, it sets ThingToBitmapConverter.Provider = new SpriteProvider(_loadedSprFile);
                // But SpriteProvider only provides sprites, not the composition of an item.
                
                // Let's try to fetch the ThingType from the MainViewModel's static context if available, 
                // or better, let's inject a service.
                
                // Since I cannot easily change the architecture right now, I will create a static helper in MainViewModel 
                // or a Service that can provide the ThingType or Bitmap given a ClientId.
                
                if (ItemImageService.Instance != null)
                {
                    return ItemImageService.Instance.GetImage(item.ClientId);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
