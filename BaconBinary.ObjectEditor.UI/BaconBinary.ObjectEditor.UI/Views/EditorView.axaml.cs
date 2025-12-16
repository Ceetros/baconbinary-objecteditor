using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using BaconBinary.ObjectEditor.UI.ViewModels;
using System.Threading.Tasks;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
        }

        private async void OnSpriteDrag(object sender, PointerPressedEventArgs e)
        {
            if (sender is not Border { DataContext: uint spriteId }) return;

            var dragData = new DataObject();
            dragData.Set("SpriteId", spriteId);

            await DragDrop.DoDragDrop(e, dragData, DragDropEffects.Copy);
        }

        private void OnSlotDragEnter(object sender, DragEventArgs e)
        {
            if (sender is Border border && e.Data.Contains("SpriteId"))
            {
                border.BorderBrush = Brushes.LawnGreen;
                border.BorderThickness = new Avalonia.Thickness(2);
            }
        }

        private void OnSlotDragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.BorderBrush = (IBrush)this.FindResource("BorderBrush");
                border.BorderThickness = new Avalonia.Thickness(1);
            }
        }

        private void OnSlotDrop(object sender, DragEventArgs e)
        {
            if (sender is not Border { DataContext: SpriteSlotViewModel slotVm } border) return;
            
            border.BorderBrush = (IBrush)this.FindResource("BorderBrush");
            border.BorderThickness = new Avalonia.Thickness(1);

            if (e.Data.Get("SpriteId") is uint spriteId && DataContext is MainViewModel mainVm)
            {
                mainVm.UpdateSpriteSlotCommand.Execute(new object[] { slotVm, spriteId });
            }
        }

        private void OnResizeLostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ResizeObjectCommand.Execute(null);
            }
        }
    }
}
