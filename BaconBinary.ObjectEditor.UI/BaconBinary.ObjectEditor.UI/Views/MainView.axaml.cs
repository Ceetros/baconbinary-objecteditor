using Avalonia.Controls;
using Avalonia.Input;
using BaconBinary.ObjectEditor.UI.ViewModels;
using System.Linq;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void EditItem_OnDoubleTapped(object sender, TappedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.EditItemCommand.Execute(null);
            }
        }

        public void OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.FileNames))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        public void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetFileNames()?.FirstOrDefault() is { } filePath)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ImportSheetCommand.Execute(filePath);
                }
                e.Handled = true;
            }
        }
    }
}
