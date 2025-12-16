using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BaconBinary.Core;
using BaconBinary.Core.IO.Dat;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class OpenFilesViewModel : ObservableObject
    {
        public IStorageProvider StorageProvider { get; set; }
        
        [ObservableProperty] private string _datPath;
        [ObservableProperty] private string _sprPath;
        [ObservableProperty] private string _detectedVersion = "Unknown";
        [ObservableProperty] private bool _isSprPathEnabled = true;
        
        [ObservableProperty] private bool _isEncrypted;
        [ObservableProperty] private string _encryptionKey;
        
        [ObservableProperty] private bool _showTransparencyOption;
        [ObservableProperty] private bool _useTransparency;

        public bool Success { get; private set; } = false;

        partial void OnDatPathChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (value.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase))
            {
                SprPath = string.Empty;
                IsSprPathEnabled = false;
                ShowTransparencyOption = false;
                DetectMetaInfo(value);
            }
            else
            {
                IsSprPathEnabled = true;
                ShowTransparencyOption = true;
                DetectDatInfo(value);
                
                // Auto-select .spr if it exists
                string sprCandidate = Path.ChangeExtension(value, ".spr");
                if (File.Exists(sprCandidate))
                {
                    SprPath = sprCandidate;
                }
            }
        }

        private void DetectMetaInfo(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                using var reader = new BinaryReader(fs);
                
                fs.Seek(6, SeekOrigin.Begin);
                
                byte encryptionType = reader.ReadByte();
                IsEncrypted = encryptionType == 1;

                DetectedVersion = "BSUIT Project";
            }
            catch
            {
                DetectedVersion = "Error reading file";
            }
        }

        private void DetectDatInfo(string path)
        {
            IsEncrypted = false;
            EncryptionKey = string.Empty;
            
            try
            {
                string version = ClientVersionRepository.DetectVersion(path);
                DetectedVersion = string.IsNullOrEmpty(version) ? "Unknown" : version;
                
                // Auto-detect transparency based on version (7.70+ usually has it)
                if (double.TryParse(version, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
                {
                    UseTransparency = v >= 7.70;
                }
            }
            catch
            {
                DetectedVersion = "Error reading file";
            }
        }

        [RelayCommand]
        private async Task BrowseDat()
        {
            var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Tibia.dat or project.meta",
                FileTypeFilter = new[] 
                { 
                    new FilePickerFileType("Tibia Dat") { Patterns = new[] { "*.dat" } },
                    new FilePickerFileType("BaconBinary Meta") { Patterns = new[] { "*.meta" } },
                    new FilePickerFileType("All") { Patterns = new[] { "*.*" } }
                }
            });

            if (result.Count > 0)
            {
                DatPath = result[0].Path.LocalPath;
            }
        }

        [RelayCommand]
        private async Task BrowseSpr()
        {
            var result = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Tibia.spr",
                FileTypeFilter = new[] { new FilePickerFileType("Tibia Spr") { Patterns = new[] { "*.spr" } } }
            });

            if (result.Count > 0)
            {
                SprPath = result[0].Path.LocalPath;
            }
        }

        [RelayCommand]
        private void Load(Window window)
        {
            if (string.IsNullOrEmpty(DatPath) || (IsSprPathEnabled && string.IsNullOrEmpty(SprPath)))
            {
                return;
            }

            if (IsEncrypted && string.IsNullOrWhiteSpace(EncryptionKey))
            {
                return;
            }
            
            Success = true;
            window.Close();
        }
    }
}
