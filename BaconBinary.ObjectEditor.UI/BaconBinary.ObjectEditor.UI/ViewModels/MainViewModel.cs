using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using BaconBinary.Core.Configurations;
using BaconBinary.Core.Enum; 
using BaconBinary.Core.IO.Dat;
using BaconBinary.Core.IO.Spr;
using BaconBinary.Core.IO.Meta;
using BaconBinary.Core.IO.Asset;
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
        private readonly MetaReader _metaReader = new();
        private readonly DatWriter _datWriter = new();
        private readonly SprWriter _sprWriter = new();
        private readonly MetaWriter _metaWriter = new();
        private readonly AssetWriter _assetWriter = new();
        
        private readonly DispatcherTimer _animationTimer;
        
        // Editor State
        [ObservableProperty] private int _currentFrameIndex = 0;
        [ObservableProperty] private int _currentDirection = 2; // Pattern X
        [ObservableProperty] private int _currentPatternY = 0;
        [ObservableProperty] private int _currentPatternZ = 0;
        [ObservableProperty] private bool _isPlaying;
        [ObservableProperty] private FrameGroupType _selectedFrameGroup = FrameGroupType.Default;
        public ObservableCollection<SpriteSlotViewModel> ComposerSlots { get; } = new();
        [ObservableProperty] private int _composerWidth = 32;
        [ObservableProperty] private bool _isDirty = false;
        [ObservableProperty] private ThingType _tempProps;

        [ObservableProperty] private string _versionString = "8.60";
        [ObservableProperty] private int _itemCount;
        [ObservableProperty] private int _outfitCount;
        [ObservableProperty] private int _effectCount;
        [ObservableProperty] private int _missileCount;
        [ObservableProperty] private uint _spriteCount;

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
        private SprFile _loadedSprFile;
        private string _currentDatPath;
        private string _currentSprPath;

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
                Console.WriteLine("Show 'Save Changes?' dialog here.");
            }
            IsDirty = false;
        }

        partial void OnSelectedItemChanged(ThingType value)
        {
            CurrentFrameIndex = 0;
            CurrentDirection = 2;
            CurrentPatternY = 0;
            CurrentPatternZ = 0;
            IsPlaying = false;
            _animationTimer.Stop();
            
            if (value != null)
            {
                TempProps = value.Clone();
            }
            else
            {
                TempProps = null;
            }
            
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
        partial void OnCurrentPatternYChanged(int value) => UpdateComposerSlots();
        partial void OnCurrentPatternZChanged(int value) => UpdateComposerSlots();
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
        }

        private void UpdateComposerSlots()
        {
            ComposerSlots.Clear();
            if (SelectedItem == null || !SelectedItem.FrameGroups.ContainsKey(SelectedFrameGroup)) return;

            var group = SelectedItem.FrameGroups[SelectedFrameGroup];
            
            bool isItem = SelectedItem.Category == ThingCategory.Item;

            if (isItem)
            {
                ComposerWidth = group.PatternX * group.Width * 32;
                for (int py = 0; py < group.PatternY; py++)
                for (int px = 0; px < group.PatternX; px++)
                for (int y = 0; y < group.Height; y++)
                for (int x = 0; x < group.Width; x++)
                {
                    uint spriteId = group.GetSpriteId(CurrentFrameIndex, px, py, CurrentPatternZ, 0, x, y);
                    ComposerSlots.Add(new SpriteSlotViewModel(x, y, spriteId));
                }
            }
            else
            {
                ComposerWidth = group.Width * 32;
                for (int y = 0; y < group.Height; y++)
                for (int x = 0; x < group.Width; x++)
                {
                    uint spriteId = group.GetSpriteId(CurrentFrameIndex, CurrentDirection, 0, CurrentPatternZ, 0, x, y);
                    ComposerSlots.Add(new SpriteSlotViewModel(x, y, spriteId));
                }
            }
        }
        
        [RelayCommand]
        private void UpdateSpriteSlot(object[] parameters)
        {
            if (parameters.Length != 2 || 
                parameters[0] is not SpriteSlotViewModel slotVm || 
                parameters[1] is not uint newSpriteId) return;

            if (TempProps == null || !TempProps.FrameGroups.ContainsKey(SelectedFrameGroup)) return;
            var group = TempProps.FrameGroups[SelectedFrameGroup];

            int index = ComposerSlots.IndexOf(slotVm);
            if (index == -1) return;

            bool isItem = TempProps.Category == ThingCategory.Item;
            
            int targetPatternX, targetPatternY, targetX, targetY;

            if (isItem)
            {
                int slotsPerItem = group.Width * group.Height;
                int itemsPerRow = group.PatternX;
                int totalSlotsPerRow = slotsPerItem * itemsPerRow;

                int rowIndex = index / totalSlotsPerRow;
                int colIndexInRow = index % totalSlotsPerRow;

                targetPatternY = rowIndex;
                targetPatternX = colIndexInRow / slotsPerItem;
                
                int slotInItem = colIndexInRow % slotsPerItem;
                targetY = slotInItem / group.Width;
                targetX = slotInItem % group.Width;
            }
            else
            {
                targetPatternX = CurrentDirection;
                targetPatternY = 0;
                
                targetY = index / group.Width;
                targetX = index % group.Width;
            }

            group.SetSpriteId(CurrentFrameIndex, targetPatternX, targetPatternY, CurrentPatternZ, 0, targetX, targetY, newSpriteId);
            
            slotVm.SpriteId = newSpriteId;
            IsDirty = true;
        }

        [RelayCommand]
        private void ResizeObject()
        {
            if (TempProps == null || !TempProps.FrameGroups.ContainsKey(SelectedFrameGroup)) return;
            
            var group = TempProps.FrameGroups[SelectedFrameGroup];
            group.Resize(TempProps.Width, TempProps.Height, TempProps.Layers, TempProps.PatternX, TempProps.PatternY, TempProps.PatternZ, (byte)TempProps.Frames);
            
            UpdateComposerSlots();
            IsDirty = true;
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
                SprPath = session.SprPath
            };
            await ShowOpenFilesDialog(desktop.MainWindow, vm);
        }

        private async Task ShowOpenFilesDialog(TopLevel topLevel, OpenFilesViewModel preloadedVm = null)
        {
            string datPath = null, sprPath = null, version = null, key = null;
            bool useTransparency = false;

            if (topLevel is Window window)
            {
                var vm = preloadedVm ?? new OpenFilesViewModel { StorageProvider = topLevel.StorageProvider };
                var dialog = new OpenFilesWindow { DataContext = vm };
                await dialog.ShowDialog(window);
                if (!vm.Success) return;
                
                datPath = vm.DatPath;
                sprPath = vm.SprPath;
                version = vm.DetectedVersion;
                key = vm.EncryptionKey;
                useTransparency = vm.UseTransparency;
            }
            else
            {
                // Browser logic...
            }

            await LoadProject(datPath, sprPath, version, key, useTransparency);
        }

        public async Task LoadProject(string primaryPath, string secondaryPath, string version, string key, bool useTransparency = false)
        {
            StatusText = "Loading assets...";
            IsLoading = true;

            await Task.Run(async () =>
            {
                try
                {
                    if (primaryPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    {
                        var (dat, spr) = _metaReader.ReadMetaFile(primaryPath, key);
                        _loadedDatFile = dat;
                        _loadedSprFile = spr;
                        _currentDatPath = primaryPath;
                        _currentSprPath = null;
                    }
                    else
                    {
                        // Set features BEFORE reading
                        ushort versionNumber = ushort.Parse(version.Replace(".", ""));
                        ClientFeatures.SetVersion(versionNumber);
                        ClientFeatures.Transparency = useTransparency;

                        _loadedDatFile = _datReader.ReadDatFile(primaryPath, version);
                        _loadedSprFile = _sprReader.ReadSprFile(secondaryPath, version);
                        _currentDatPath = primaryPath;
                        _currentSprPath = secondaryPath;
                    }

                    ThingToBitmapConverter.Provider = new SpriteProvider(_loadedSprFile);

                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        ItemCount = _loadedDatFile.Items.Count;
                        OutfitCount = _loadedDatFile.Outfits.Count;
                        EffectCount = _loadedDatFile.Effects.Count;
                        MissileCount = _loadedDatFile.Missiles.Count;
                        SpriteCount = _loadedSprFile.SpriteCount;
                        VersionString = _loadedDatFile.Version;
                        IsTransparency = ClientFeatures.Transparency;
                        IsExtended = ClientFeatures.Extended;

                        TotalSpritePages = (int)Math.Ceiling((double)SpriteCount / SpritesPerPage);
                        CurrentSpritePage = 1;
                        LoadSpritePage();

                        StatusText = "Project loaded successfully.";
                        IsLoading = false;
                        
                        SelectedCategoryIndex = 0;
                        RefreshList();
                    });
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog("Failed to load project", ex.Message);
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => IsLoading = false);
                }
            });
        }

        [RelayCommand]
        public async Task CompileProject()
        {
            if (_loadedDatFile == null || _loadedSprFile == null) return;
            await ExecuteCompilation(_currentDatPath, _currentSprPath, false, null);
        }

        [RelayCommand]
        public async Task CompileProjectAs(object parameter)
        {
            if (_loadedDatFile == null || _loadedSprFile == null) return;
            if (parameter is not Visual visual) return;
            var topLevel = TopLevel.GetTopLevel(visual);
            if (topLevel == null) return;

            var optionsVm = new CompileOptionsViewModel();
            var optionsDialog = new CompileOptionsWindow { DataContext = optionsVm };
            if (topLevel is Window parentWindow) await optionsDialog.ShowDialog(parentWindow);

            if (!optionsVm.Success) return;

            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Output Folder"
            });

            if (folder.Count > 0)
            {
                string outputPath = folder[0].Path.LocalPath;
                
                if (optionsVm.IsStandardFormat)
                {
                    string datPath = Path.Combine(outputPath, "Tibia.dat");
                    string sprPath = Path.Combine(outputPath, "Tibia.spr");
                    await ExecuteCompilation(datPath, sprPath, false, null);
                }
                else // BSUIT Format
                {
                    string metaPath = Path.Combine(outputPath, "project.meta");
                    string assetPath = Path.Combine(outputPath, "project.asset"); // Base path for assets
                    await ExecuteCompilation(metaPath, assetPath, true, optionsVm.EncryptionKey);
                }
            }
        }

        private async Task ExecuteCompilation(string primaryPath, string secondaryPath, bool isBsuit, string key)
        {
            StatusText = "Compiling project...";
            IsLoading = true;

            await Task.Run(async () =>
            {
                try
                {
                    if (isBsuit)
                    {
                        _metaWriter.WriteMetaFile(_loadedDatFile, _loadedSprFile, primaryPath, !string.IsNullOrEmpty(key), key);
                        _assetWriter.WriteAssetFiles(_loadedSprFile, secondaryPath, !string.IsNullOrEmpty(key), key);
                    }
                    else
                    {
                        _datWriter.WriteDatFile(_loadedDatFile, primaryPath);
                        _sprWriter.WriteSprFile(_loadedSprFile, secondaryPath, _loadedDatFile.Version);
                    }
                    
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        StatusText = "Project compiled successfully.";
                        IsLoading = false;
                    });
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog("Compilation Failed", ex.Message);
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => IsLoading = false);
                }
            });
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var vm = new MessageBoxViewModel(title, message);
                var dialog = new MessageBoxWindow { DataContext = vm };
                
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var owner = desktop.Windows.FirstOrDefault(w => w.IsActive && w.IsVisible) ?? desktop.MainWindow;
                    
                    if (owner != null && owner.IsVisible)
                    {
                        await dialog.ShowDialog(owner);
                    }
                    else
                    {
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        dialog.Show();
                    }
                }
            });
        }

        [RelayCommand] private void OpenAbout() => new AboutWindow().Show();
        [RelayCommand] private void Donate() => OpenUrl("https://www.paypal.com/donate/?hosted_button_id=5Q8YX497C9QWU");

        [RelayCommand]
        private void EditItem()
        {
            if (SelectedItem != null)
            {
                TempProps = SelectedItem.Clone();
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
            TempProps = null;
        }

        [RelayCommand]
        private void SaveChanges()
        {
            if (SelectedItem != null && TempProps != null)
            {
                SelectedItem.ApplyProps(TempProps);
                SelectedItem.FrameGroups = TempProps.FrameGroups;
                IsDirty = true;
                StatusText = $"Item {SelectedItem.Id} saved to memory.";
            }
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
            // This method is now a legacy wrapper
            await LoadProject(datPath, sprPath, version, null, false);
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
                thing.Id = pair.Key;
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
                thing.Id = pair.Key;
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
