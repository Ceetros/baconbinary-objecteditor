using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class CompileOptionsViewModel : ObservableObject
    {
        [ObservableProperty] private bool _isStandardFormat = true;
        [ObservableProperty] private bool _isBsuitFormat;
        
        [ObservableProperty] private bool _isEncryptionEnabled;
        [ObservableProperty] private string _encryptionKey;

        public bool Success { get; private set; } = false;

        partial void OnIsStandardFormatChanged(bool value)
        {
            if (value)
            {
                IsBsuitFormat = false;
                IsEncryptionEnabled = false;
            }
        }

        partial void OnIsBsuitFormatChanged(bool value)
        {
            if (value)
            {
                IsStandardFormat = false;
            }
        }

        [RelayCommand]
        private void Confirm(Window window)
        {
            if (IsBsuitFormat && IsEncryptionEnabled && string.IsNullOrWhiteSpace(EncryptionKey))
            {
                return;
            }

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
