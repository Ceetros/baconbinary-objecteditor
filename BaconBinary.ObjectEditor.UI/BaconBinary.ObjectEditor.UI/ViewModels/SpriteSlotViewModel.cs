using CommunityToolkit.Mvvm.ComponentModel;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class SpriteSlotViewModel : ObservableObject
    {
        [ObservableProperty]
        private uint _spriteId;

        public int X { get; }
        public int Y { get; }

        public SpriteSlotViewModel(int x, int y, uint spriteId)
        {
            X = x;
            Y = y;
            SpriteId = spriteId;
        }
    }
}
