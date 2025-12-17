using Avalonia.Media.Imaging;
using BaconBinary.Core.IO.Dat;
using BaconBinary.Core.Models;
using BaconBinary.ObjectEditor.UI.Converters;

namespace BaconBinary.ObjectEditor.UI.Services
{
    public class ItemImageService
    {
        public static ItemImageService Instance { get; private set; }

        private readonly DatFile _datFile;

        public ItemImageService(DatFile datFile)
        {
            _datFile = datFile;
            Instance = this;
        }

        public Bitmap GetImage(ushort clientId)
        {
            if (_datFile.Items.TryGetValue(clientId, out var thingType))
            {
                return ThingToBitmapConverter.Provider.GetThingBitmap(thingType);
            }
            return null;
        }
    }
}
