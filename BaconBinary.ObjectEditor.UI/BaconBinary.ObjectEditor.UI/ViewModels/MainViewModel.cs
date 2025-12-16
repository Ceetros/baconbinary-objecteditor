using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging; 
using Avalonia.Threading;      
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BaconBinary.Core;
using BaconBinary.Core.Enum; 
using BaconBinary.Core.IO.Dat;
using BaconBinary.Core.IO.Spr;
using BaconBinary.Core.Models;
using BaconBinary.ObjectEditor.UI.Converters;
using BaconBinary.ObjectEditor.UI.Services;
using BaconBinary.ObjectEditor.UI.Views;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatReader _datReader = new();
        private readonly SprReader _sprReader = new();
        
        private readonly DispatcherTimer _animationTimer;
        private int _currentFrameIndex = 0;

        [ObservableProperty] private string _versionString = "8.60";
        [ObservableProperty] private int _itemCount;
        [ObservableProperty] private int _outfitCount;
        [ObservableProperty] private int _effectCount;
        [ObservableProperty] private int _missileCount;
        [ObservableProperty] private int _spriteCount;

        [ObservableProperty] private bool _isExtended;
        [ObservableProperty] private bool _isTransparency;
        [ObservableProperty] private bool _isLoading;

        [ObservableProperty] private Bitmap _previewImage;

        public ObservableCollection<ThingType> Items { get; } = new();

        [ObservableProperty] private ThingType _selectedItem;
        
        [ObservableProperty] private int _selectedCategoryIndex = 0;
        private DatFile _loadedDatFile; 

        [ObservableProperty] private string _statusText = "Ready.";

        public MainViewModel()
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200) 
            };
            _animationTimer.Tick += OnAnimationTick;
        }

        partial void OnSelectedItemChanged(ThingType value)
        {
            _currentFrameIndex = 0;
            
            if (value != null && value.FrameGroups.ContainsKey(FrameGroupType.Default))
            {
                var group = value.FrameGroups[FrameGroupType.Default];

                
                if (group.Frames > 1) 
                {
                    if (!_animationTimer.IsEnabled) _animationTimer.Start();
                }
                else
                {
                    _animationTimer.Stop();
                }
            }
            else
            {
                _animationTimer.Stop();
            }
            
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            if (SelectedItem == null || !SelectedItem.FrameGroups.ContainsKey(FrameGroupType.Default)) 
            {
                _animationTimer.Stop();
                return;
            }

            var group = SelectedItem.FrameGroups[FrameGroupType.Default];
            
            SelectedItem.FrameIndex++;
            
            if (SelectedItem.FrameIndex > group.Frames)
            {
                SelectedItem.FrameIndex = 0;
            }
        }
        
        [RelayCommand]
        public async Task OpenFiles(Window ownerWindow)
        {
            var vm = new OpenFilesViewModel();
            
            if (ownerWindow != null)
            {
                vm.StorageProvider = ownerWindow.StorageProvider;
            }
            
            var dialog = new OpenFilesWindow
            {
                DataContext = vm
            };

            if (ownerWindow != null)
                await dialog.ShowDialog(ownerWindow);
            else
                dialog.Show();

            if (vm.Success)
            {
                StatusText = "Loading assets...";
                
                string datPath = vm.DatPath;
                string sprPath = vm.SprPath;
                VersionString = vm.SelectedVersion;
                
                IsExtended = ClientFeatures.Extended;
                IsTransparency = ClientFeatures.Transparency;

                await LoadDataInternal(datPath, sprPath, VersionString);
            }
        }

        [RelayCommand]
        private void OpenAbout()
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        [RelayCommand]
        private void Donate()
        {
            var url = "https://www.paypal.com/donate/?hosted_button_id=5Q8YX497C9QWU";
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
            }
        }

        partial void OnSelectedCategoryIndexChanged(int value)
        {
            RefreshList();
        }

        private async Task LoadDataInternal(string datPath, string sprPath, string version)
        {
            IsLoading = true;
            await Task.Run(() =>
            {
                Items.Clear(); 
                
                var datFile = _datReader.ReadDatFile(datPath, version);
                var sprFile = _sprReader.ReadSprFile(sprPath, version);
                
                _loadedDatFile = datFile;

                var provider = new SpriteProvider(sprFile);
                ThingToBitmapConverter.Provider = provider;

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ItemCount = datFile.Items.Count;
                    OutfitCount = datFile.Outfits.Count;
                    EffectCount = datFile.Effects.Count;
                    MissileCount = datFile.Missiles.Count;
                    SpriteCount = sprFile.SpriteCount;

                    StatusText = "Project loaded successfully.";
                    IsLoading = false;
                    
                    SelectedCategoryIndex = 0;
                    RefreshList();
                });
            });
        }

        private void RefreshList()
        {
            if (_loadedDatFile == null) return;

            Items.Clear();
            System.Collections.Generic.IDictionary<uint, ThingType> sourceDict = null;

            switch (SelectedCategoryIndex)
            {
                case 0: sourceDict = _loadedDatFile.Items; break;
                case 1: sourceDict = _loadedDatFile.Outfits; break;
                case 2: sourceDict = _loadedDatFile.Effects; break;
                case 3: sourceDict = _loadedDatFile.Missiles; break;
            }

            if (sourceDict != null)
            {
                foreach (var pair in sourceDict)
                {
                    var id = pair.Key;
                    var thing = pair.Value;
                    thing.ID = id; 
                    Items.Add(thing);
                }
            }
        }
    }
}
