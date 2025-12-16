using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class MessageBoxViewModel : ObservableObject
    {
        [ObservableProperty] private string _title;
        [ObservableProperty] private string _message;
        [ObservableProperty] private string _buttonText = "OK";

        public MessageBoxViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }

        [RelayCommand]
        private void Close(Window window)
        {
            window.Close();
        }
    }
}
