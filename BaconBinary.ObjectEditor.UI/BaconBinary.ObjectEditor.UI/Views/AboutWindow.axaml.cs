using Avalonia.Controls;
using Avalonia.Interactivity;
using BaconBinary.Core;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class AboutWindow : Window
    {
        public string AppVersion => Definitions.Version;

        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
