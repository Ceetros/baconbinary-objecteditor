using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class KeyPromptViewModel : ObservableObject
    {
        [ObservableProperty] private string _key;
        public bool Success { get; private set; }

        [RelayCommand]
        private void Ok(Window window)
        {
            Success = true;
            window.Close();
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            Success = false;
            window.Close();
        }
    }
}
