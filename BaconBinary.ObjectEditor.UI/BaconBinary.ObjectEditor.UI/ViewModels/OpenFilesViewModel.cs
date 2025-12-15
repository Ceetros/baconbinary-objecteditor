using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BaconBinary.Core;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class OpenFilesViewModel : ObservableObject
    {
        public bool Success { get; private set; } = false;

        [ObservableProperty] private string _clientPath;
        [ObservableProperty] private string _datPath;
        [ObservableProperty] private string _sprPath;

        [ObservableProperty] private string _selectedVersion;
        public ObservableCollection<string> Versions { get; } = new()
        {
            "7.10", "7.40", "7.60", "8.60", "9.60", "10.98"
        };

        [ObservableProperty] private bool _isExtended;
        [ObservableProperty] private bool _isTransparency;
        [ObservableProperty] private bool _isFrameDurations;
        [ObservableProperty] private bool _isFrameGroups;

        [ObservableProperty] private string _statusMessage = "Select a folder containing the asset files.";
        [ObservableProperty] private bool _hasError;

        public IStorageProvider StorageProvider { get; set; }

        public OpenFilesViewModel()
        {
            SelectedVersion = "8.60";
        }

        partial void OnSelectedVersionChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            int v = int.Parse(value.Replace(".", ""));
            
            ClientFeatures.SetVersion(v);

            IsExtended = ClientFeatures.Extended;
            IsTransparency = ClientFeatures.Transparency;
            IsFrameGroups = ClientFeatures.FrameGroups;
            IsFrameDurations = ClientFeatures.FrameDurations;
        }

        [RelayCommand]
        public async Task Browse()
        {
            if (StorageProvider == null) return;

            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Client Folder",
                AllowMultiple = false
            });

            if (result.Count > 0)
            {
                ClientPath = result[0].Path.LocalPath;
                AutoDetectFiles(ClientPath);
            }
        }

        private void AutoDetectFiles(string folder)
        {
            var files = Directory.GetFiles(folder);
            var dat = files.FirstOrDefault(f => f.EndsWith(".dat", System.StringComparison.OrdinalIgnoreCase));
            var spr = files.FirstOrDefault(f => f.EndsWith(".spr", System.StringComparison.OrdinalIgnoreCase));

            if (dat != null) DatPath = dat;
            if (spr != null) SprPath = spr;

            if (dat != null && spr != null)
            {
                var detectedVersion = ClientVersionRepository.DetectVersion(dat);
        
                if (detectedVersion != null)
                {
                    SelectedVersion = detectedVersion;
                    StatusMessage = $"Files detected. Version detected: {detectedVersion}";
                }
                else
                {
                    StatusMessage = "Files detected, but version signature is unknown.";
                }
        
                HasError = false;
                
                UpdateSignaturesDisplay(dat, spr);
            }
            else
            {
                StatusMessage = "Could not find .dat or .spr files.";
                HasError = true;
            }
        }
        
        private void UpdateSignaturesDisplay(string datPath, string sprPath)
        {
        }

        [RelayCommand]
        public void Load(Avalonia.Controls.Window window)
        {
            if (string.IsNullOrEmpty(DatPath) || string.IsNullOrEmpty(SprPath))
            {
                StatusMessage = "Please select valid .dat and .spr files.";
                HasError = true;
                return;
            }

            ClientFeatures.Extended = IsExtended; 
            ClientFeatures.FrameDurations = IsFrameDurations; 
            ClientFeatures.FrameGroups = IsFrameGroups; 
            ClientFeatures.Transparency = IsTransparency; 

            Success = true;
            window.Close();
        }

        [RelayCommand]
        public void Cancel(Avalonia.Controls.Window window)
        {
            Success = false;
            window.Close();
        }
        
    }
}