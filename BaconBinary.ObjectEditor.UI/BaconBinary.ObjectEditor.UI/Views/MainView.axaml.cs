using Avalonia.Controls;
using Avalonia.Input;
using BaconBinary.ObjectEditor.UI.ViewModels;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void EditItem_OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.EditItemCommand.Execute(null);
            }
        }
    }
}
