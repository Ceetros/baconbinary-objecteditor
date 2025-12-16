using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia;
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
        
        // Editor State
        [ObservableProperty] private int _currentFrameIndex = 0;
        [ObservableProperty] private int _currentDirection = 2;
        [ObservableProperty] private bool _isPlaying;
        [ObservableProperty] private FrameGroupType _selectedFrameGroup = FrameGroupType.Default;
        public ObservableCollection<SpriteSlotViewModel> ComposerSlots { get; } = new();
        [ObservableProperty] private int _composerWidth = 32;
        [ObservableProperty] private bool _isDirty = false;

        [ObservableProperty] private string _versionString = "8.60";
        [ObservableProperty] private int _itemCount;
        [ObservableProperty] private int _outfitCount;
        [ObservableProperty] private int _effectCount;
        [ObservableProperty] private int _missileCount;
        [ObservableProperty] private int _spriteCount;

        [ObservableProperty] private bool _isExtended;
        [ObservableProperty] private bool _isTransparency;
        [ObservableProperty] private bool _isLoading;
        
        [ObservableProperty] private bool _isEditing = false;

        public ObservableCollection<ThingType> Items { get; } = new();
        public ObservableCollection<ThingType> EditorItems { get; } = new();
        public ObservableCollection<uint> SpriteIds { get; } = new();

        [ObservableProperty] private int _currentItemPage = 1;
        [ObservableProperty] private int _totalItemPages = 1;
        private const int ItemsPerPage = 100;

        [ObservableProperty] private int _currentSpritePage = 1;
        [ObservableProperty] private int _totalSpritePages = 1;
        private const int SpritesPerPage = 100;

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

        async partial void OnSelectedItemChanging(ThingType value)
        {
            if (IsEditing && IsDirty)
            {
                // TODO: Implement a proper dialog service
                Console.WriteLine("Show 'Save Changes?' dialog here.");
            }
            IsDirty = false;
        }

        partial void OnSelectedItemChanged(ThingType value)
        {
            CurrentFrameIndex = 0;
            CurrentDirection = 2;
            IsPlaying = false;
            _animationTimer.Stop();
            
            UpdateComposerSlots();
            
            if (value != null && value.FrameGroups.ContainsKey(SelectedFrameGroup))
            {
                var group = value.FrameGroups[SelectedFrameGroup];
                if (group.Frames > 1 && !IsEditing) 
                {
                     _animationTimer.Start();
                }
            }
        }

        partial void OnCurrentDirectionChanged(int value) => UpdateComposerSlots();
        partial void OnCurrentFrameIndexChanged(int value) => UpdateComposerSlots();
        partial void OnSelectedFrameGroupChanged(FrameGroupType value) => UpdateComposerSlots();

        private void OnAnimationTick(object sender, EventArgs e)
        {
            if (SelectedItem == null || !SelectedItem.FrameGroups.ContainsKey(SelectedFrameGroup)) 
            {
                _animationTimer.Stop();
                IsPlaying = false;
                return;
            }

            var group = SelectedItem.FrameGroups[SelectedFrameGroup];
            
            int nextFrame = CurrentFrameIndex + 1;
            if (nextFrame >= group.Frames)
            {
                nextFrame = 0;
            }
            
            CurrentFrameIndex = nextFrame;
            SelectedItem.FrameIndex = nextFrame;
        }

        private void UpdateComposerSlots()
        {
            ComposerSlots.Clear();
            if (SelectedItem == null || !SelectedItem.FrameGroups.ContainsKey(SelectedFrameGroup)) return;

            var group = SelectedItem.FrameGroups[SelectedFrameGroup];
            ComposerWidth = group.Width * 32;

            // Iterate visually: Top-Left to Bottom-Right
            for (int y = 0; y < group.Height; y++)
            {
                for (int x = 0; x < group.Width; x++)
                {
                    // Map visual position (x, y) to data index (inverted)
                    // This matches the logic in SpriteProvider.GetThingBitmap
                    int dataX = group.Width - 1 - x;
                    int dataY = group.Height - 1 - y;

                    uint spriteId = group.GetSpriteId(CurrentFrameIndex, CurrentDirection, 0, 0, 0, dataX, dataY);
                    ComposerSlots.Add(new SpriteSlotViewModel(x, y, spriteId));
                }
            }
        }
        
        [RelayCommand]
        public void ToggleAnimation()
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying) _animationTimer.Start();
            else _animationTimer.Stop();
        }

        [RelayCommand]
        public void SetDirection(string direction)
        {
            switch (direction.ToLower())
            {
                case "north": CurrentDirection = 0; break;
                case "east": CurrentDirection = 1; break;
                case "south": CurrentDirection = 2; break;
                case "west": CurrentDirection = 3; break;
            }
        }
        
        [RelayCommand]
        public async Task OpenFiles(object parameter)
        {
            if (parameter is not Visual visual) return;
            var topLevel = TopLevel.GetTopLevel(visual);
            if (topLevel == null) return;
            await ShowOpenFilesDialog(topLevel);
        }

        public async void ShowPreloadedOpenDialog(SessionState session)
        {
            if (App.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow == null) return;
            var vm = new OpenFilesViewModel
            {
                StorageProvider = desktop.MainWindow.StorageProvider,
                DatPath = session.DatPath,
                SprPath = session.SprPath,
                SelectedVersion = session.Version
            };
            await ShowOpenFilesDialog(desktop.MainWindow, vm);
        }

        private async Task ShowOpenFilesDialog(TopLevel topLevel, OpenFilesViewModel preloadedVm = null)
        {
            string datPath = null, sprPath = null, version = null;

            if (topLevel is Window window)
            {
                var vm = preloadedVm ?? new OpenFilesViewModel { StorageProvider = topLevel.StorageProvider };
                var dialog = new OpenFilesWindow { DataContext = vm };
                await dialog.ShowDialog(window);
                if (!vm.Success) return;
                datPath = vm.DatPath;
                sprPath = vm.SprPath;
                version = vm.SelectedVersion;
            }
            else
            {
                // Browser logic...
            }

            await LoadProject(datPath, sprPath, version);
        }

        public async Task LoadProject(string datPath, string sprPath, string version)
        {
            StatusText = "Loading assets...";
            VersionString = version;
            IsExtended = ClientFeatures.Extended;
            IsTransparency = ClientFeatures.Transparency;
            await LoadDataInternal(datPath, sprPath, version);
            SessionManager.SaveSession(datPath, sprPath, version);
        }

        [RelayCommand] private void OpenAbout() => new AboutWindow().Show();
        [RelayCommand] private void Donate() => OpenUrl("https://www.paypal.com/donate/?hosted_button_id=5Q8YX497C9QWU");

        [RelayCommand]
        private void EditItem()
        {
            if (SelectedItem != null)
            {
                IsEditing = true;
                LoadItemPage(1);
                UpdateComposerSlots();
            }
        }

        [RelayCommand]
        private void ExitEditMode()
        {
            IsEditing = false;
            _animationTimer.Stop();
            IsPlaying = false;
        }

        [RelayCommand]
        private void ChangeSpritePage(string direction)
        {
            int newPage = CurrentSpritePage;
            if (direction == "next" && CurrentSpritePage < TotalSpritePages) newPage++;
            else if (direction == "prev" && CurrentSpritePage > 1) newPage--;
            if (newPage != CurrentSpritePage)
            {
                CurrentSpritePage = newPage;
                LoadSpritePage();
            }
        }
        
        [RelayCommand]
        private void ChangeItemPage(string direction)
        {
            int newPage = CurrentItemPage;
            if (direction == "next" && CurrentItemPage < TotalItemPages) newPage++;
            else if (direction == "prev" && CurrentItemPage > 1) newPage--;
            if (newPage != CurrentItemPage)
            {
                CurrentItemPage = newPage;
                LoadItemPage(CurrentItemPage);
            }
        }

        partial void OnSelectedCategoryIndexChanged(int value)
        {
            RefreshList();
            if (IsEditing) LoadItemPage(1);
        }

        private async Task LoadDataInternal(string datPath, string sprPath, string version)
        {
            IsLoading = true;
            await Task.Run(() =>
            {
                var datFile = _datReader.ReadDatFile(datPath, version);
                var sprFile = _sprReader.ReadSprFile(sprPath, version);
                _loadedDatFile = datFile;
                ThingToBitmapConverter.Provider = new SpriteProvider(sprFile);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ItemCount = datFile.Items.Count;
                    OutfitCount = datFile.Outfits.Count;
                    EffectCount = datFile.Effects.Count;
                    MissileCount = datFile.Missiles.Count;
                    SpriteCount = sprFile.SpriteCount;

                    TotalSpritePages = (int)Math.Ceiling((double)SpriteCount / SpritesPerPage);
                    CurrentSpritePage = 1;
                    LoadSpritePage();

                    StatusText = "Project loaded successfully.";
                    IsLoading = false;
                    
                    SelectedCategoryIndex = 0;
                    RefreshList();
                });
            });
        }

        private void LoadSpritePage()
        {
            SpriteIds.Clear();
            uint startId = (uint)((CurrentSpritePage - 1) * SpritesPerPage) + 1;
            uint endId = Math.Min((uint)CurrentSpritePage * SpritesPerPage, (uint)SpriteCount);
            for (uint i = startId; i <= endId; i++) SpriteIds.Add(i);
        }

        private void LoadItemPage(int page)
        {
            EditorItems.Clear();
            var sourceDict = GetCurrentCategoryDictionary();
            if (sourceDict == null) return;

            TotalItemPages = (int)Math.Ceiling((double)sourceDict.Count / ItemsPerPage);
            CurrentItemPage = page;

            var pageItems = sourceDict.Skip((page - 1) * ItemsPerPage).Take(ItemsPerPage);
            foreach (var pair in pageItems)
            {
                var thing = pair.Value;
                thing.ID = pair.Key;
                EditorItems.Add(thing);
            }
        }

        private void RefreshList()
        {
            Items.Clear();
            var sourceDict = GetCurrentCategoryDictionary();
            if (sourceDict == null) return;
            foreach (var pair in sourceDict)
            {
                var thing = pair.Value;
                thing.ID = pair.Key;
                Items.Add(thing);
            }
        }

        private IDictionary<uint, ThingType> GetCurrentCategoryDictionary()
        {
            if (_loadedDatFile == null) return null;
            return SelectedCategoryIndex switch
            {
                0 => _loadedDatFile.Items,
                1 => _loadedDatFile.Outfits,
                2 => _loadedDatFile.Effects,
                3 => _loadedDatFile.Missiles,
                _ => null
            };
        }
        
        private void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch { /* Handle exceptions for different platforms */ }
        }
    }
}
