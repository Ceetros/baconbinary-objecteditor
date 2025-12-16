using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using BaconBinary.ObjectEditor.UI.ViewModels;
using System.Linq;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class ImportSheetWindow : Window
    {
        public ImportSheetWindow()
        {
            InitializeComponent();
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (sender is Border border && e.Data.Contains(DataFormats.FileNames))
            {
                border.Background = Brushes.DarkSlateGray;
            }
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = (IBrush)this.FindResource("PanelBackgroundBrush");
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = (IBrush)this.FindResource("PanelBackgroundBrush");
            }

            if (e.Data.GetFileNames()?.FirstOrDefault() is { } filePath && DataContext is ImportSheetViewModel vm)
            {
                vm.FilePath = filePath;
            }
        }
    }
}
