using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class OpenFilesWindow : Window
    {
        public OpenFilesWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}