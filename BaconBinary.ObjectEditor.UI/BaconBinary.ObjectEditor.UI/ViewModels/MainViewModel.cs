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
using SkiaSharp;
using System.Collections.Concurrent;

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

        public ObservableCollection<ThingType> AllItems { get; } = new();
        public ObservableCollection<ThingType> MainViewItems { get; } = new();
        public ObservableCollection<ThingType> EditorItems { get; } = new();
        public ObservableCollection<uint> SpriteIds { get; } = new();

        // Pagination Properties
        [ObservableProperty] private int _mainViewCurrentPage = 1;
        [ObservableProperty] private int _mainViewTotalPages = 1;
        [ObservableProperty] private int _mainViewItemsPerPage = 200;
        public List<int> MainViewItemsPerPageOptions { get; } = new() { 100, 200, 500, 1000, 5000, 10000 };

        [ObservableProperty] private int _editorCurrentPage = 1;
        [ObservableProperty] private int _editorTotalPages = 1;
        [ObservableProperty] private int _editorItemsPerPage = 100;
        public List<int> EditorItemsPerPageOptions { get; } = new() { 100, 200, 500, 1000, 5000, 10000 };

        [ObservableProperty] private int _currentSpritePage = 1;
        [ObservableProperty] private int _totalSpritePages = 1;
        [ObservableProperty] private int _spritesPerPage = 100;
        public List<int> SpritesPerPageOptions { get; } = new() { 100, 200, 500, 1000, 5000, 10000 };

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

        partial void OnMainViewItemsPerPageChanged(int value)
        {
            if (_loadedDatFile != null) LoadMainViewPage(1);
        }

        partial void OnEditorItemsPerPageChanged(int value)
        {
            if (_loadedDatFile != null && IsEditing) LoadEditorPage(1);
        }

        partial void OnSpritesPerPageChanged(int value)
        {
            if (_loadedSprFile != null)
            {
                TotalSpritePages = (int)Math.Ceiling((double)SpriteCount / SpritesPerPage);
                LoadSpritePage(1);
            }
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
            var target = IsEditing && TempProps != null ? TempProps : SelectedItem;
            
            if (target == null || !target.FrameGroups.ContainsKey(SelectedFrameGroup)) 
            {
                _animationTimer.Stop();
                IsPlaying = false;
                return;
            }

            var group = target.FrameGroups[SelectedFrameGroup];
            
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
            
            var target = IsEditing && TempProps != null ? TempProps : SelectedItem;
            
            if (target == null || !target.FrameGroups.ContainsKey(SelectedFrameGroup)) return;

            var group = target.FrameGroups[SelectedFrameGroup];
            
            bool isItem = target.Category == ThingCategory.Item;

            if (isItem)
            {
                ComposerWidth = group.PatternX * group.Width * 32;
                for (int py = 0; py < group.PatternY; py++)
                for (int px = 0; px < group.PatternX; px++)
                for (int y = 0; y < group.Height; y++)
                for (int x = 0; x < group.Width; x++)
                {
                    int dataX = group.Width - 1 - x;
                    int dataY = group.Height - 1 - y;
                    uint spriteId = group.GetSpriteId(CurrentFrameIndex, px, py, CurrentPatternZ, 0, dataX, dataY);
                    ComposerSlots.Add(new SpriteSlotViewModel(x, y, spriteId));
                }
            }
            else
            {
                ComposerWidth = group.Width * 32;
                for (int y = 0; y < group.Height; y++)
                for (int x = 0; x < group.Width; x++)
                {
                    int dataX = group.Width - 1 - x;
                    int dataY = group.Height - 1 - y;
                    uint spriteId = group.GetSpriteId(CurrentFrameIndex, CurrentDirection, 0, CurrentPatternZ, 0, dataX, dataY);
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

            var target = IsEditing && TempProps != null ? TempProps : SelectedItem;

            if (target == null || !target.FrameGroups.ContainsKey(SelectedFrameGroup)) return;
            var group = target.FrameGroups[SelectedFrameGroup];

            int index = ComposerSlots.IndexOf(slotVm);
            if (index == -1) return;

            bool isItem = target.Category == ThingCategory.Item;
            
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
                int visualY = slotInItem / group.Width;
                int visualX = slotInItem % group.Width;
                
                targetX = group.Width - 1 - visualX;
                targetY = group.Height - 1 - visualY;
            }
            else
            {
                targetPatternX = CurrentDirection;
                targetPatternY = 0;
                
                int visualY = index / group.Width;
                int visualX = index % group.Width;
                
                targetX = group.Width - 1 - visualX;
                targetY = group.Height - 1 - visualY;
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
                        LoadSpritePage(1);

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

        [RelayCommand]
        private Task OpenImportWindow() => ImportSheet(null);

        [RelayCommand]
        public async Task ImportSprites()
        {
            if (_loadedDatFile == null || _loadedSprFile == null)
            {
                await ShowErrorDialog("Error", "Please open a project first.");
                return;
            }

            var topLevel = TopLevel.GetTopLevel((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Sprite Sheet",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count == 0) return;
            string filePath = files[0].Path.LocalPath;

            StatusText = "Importing sprites...";
            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    using var imageStream = File.OpenRead(filePath);
                    using var skBitmap = SKBitmap.Decode(imageStream);

                    SKBitmap bgraBitmap = skBitmap;
                    if (skBitmap.ColorType != SKColorType.Bgra8888)
                    {
                        bgraBitmap = new SKBitmap(skBitmap.Width, skBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                        using var canvas = new SKCanvas(bgraBitmap);
                        canvas.DrawBitmap(skBitmap, 0, 0);
                    }

                    int cols = bgraBitmap.Width / 32;
                    int rows = bgraBitmap.Height / 32;
                    int totalSprites = cols * rows;
                    
                    uint startId = _loadedSprFile.SpriteCount + 1;
                    _loadedSprFile.SpriteCount += (uint)totalSprites;

                    IntPtr pixelsPtr = bgraBitmap.GetPixels();
                    int rowBytes = bgraBitmap.RowBytes;
                    int bpp = 4;

                    var newSpritesMap = new ConcurrentDictionary<uint, Sprite>();

                    Parallel.For(0, rows, y =>
                    {
                        for (int x = 0; x < cols; x++)
                        {
                            byte[] rawPixels = new byte[4096];
                            unsafe
                            {
                                byte* srcPtr = (byte*)pixelsPtr;
                                for (int row = 0; row < 32; row++)
                                {
                                    byte* srcRow = srcPtr + ((y * 32 + row) * rowBytes) + (x * 32 * bpp);
                                    fixed (byte* destPtr = &rawPixels[row * 32 * 4])
                                    {
                                        Buffer.MemoryCopy(srcRow, destPtr, 128, 128);
                                    }
                                }
                            }

                            byte[] compressedData = SpriteCompressor.Compress(rawPixels, ClientFeatures.Transparency);
                            uint currentId = startId + (uint)(y * cols + x);
                            
                            var newSprite = new Sprite(currentId, ClientFeatures.Transparency) 
                            { 
                                CompressedPixels = compressedData, 
                                Size = (ushort)compressedData.Length 
                            };
                            
                            newSpritesMap[currentId] = newSprite;
                        }
                    });

                    if (bgraBitmap != skBitmap) bgraBitmap.Dispose();

                    for (uint id = startId; id < startId + totalSprites; id++)
                    {
                        if (newSpritesMap.TryGetValue(id, out var sprite))
                        {
                            _loadedSprFile.Sprites[id] = sprite;
                        }
                    }

                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        StatusText = $"Imported {totalSprites} sprites.";
                        SpriteCount = _loadedSprFile.SpriteCount;
                        IsLoading = false;
                        
                        // Refresh sprite palette
                        TotalSpritePages = (int)Math.Ceiling((double)SpriteCount / SpritesPerPage);
                        LoadSpritePage(TotalSpritePages); // Go to last page
                    });
                });
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Import Failed", ex.Message);
                Avalonia.Threading.Dispatcher.UIThread.Post(() => IsLoading = false);
            }
        }

        [RelayCommand]
        public async Task ImportSheet(string filePath)
        {
            if (_loadedDatFile == null || _loadedSprFile == null)
            {
                await ShowErrorDialog("Error", "Please open a project first.");
                return;
            }

            var vm = new ImportSheetViewModel(filePath);
            var dialog = new ImportSheetWindow { DataContext = vm };
            
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

            if (!vm.Success) return;

            StatusText = "Importing sprite sheet...";
            IsLoading = true;

            try
            {
                // Run heavy processing in background
                await Task.Run(() =>
                {
                    SKBitmap bgraBitmap = null;
                    
                    if (!string.IsNullOrEmpty(vm.FilePath))
                    {
                        using var imageStream = File.OpenRead(vm.FilePath);
                        using var skBitmap = SKBitmap.Decode(imageStream);

                        bgraBitmap = skBitmap;
                        if (skBitmap.ColorType != SKColorType.Bgra8888)
                        {
                            bgraBitmap = new SKBitmap(skBitmap.Width, skBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
                            using var canvas = new SKCanvas(bgraBitmap);
                            canvas.DrawBitmap(skBitmap, 0, 0);
                        }
                    }

                    var newSpritesMap = new ConcurrentDictionary<uint, Sprite>();
                    
                    // Calculate total sprites needed for the object
                    int totalSprites = vm.Width * vm.Height * vm.Layers * vm.PatternX * vm.PatternY * vm.PatternZ * vm.Frames;
                    
                    uint startId = _loadedSprFile.SpriteCount + 1;
                    _loadedSprFile.SpriteCount += (uint)totalSprites;

                    var tasks = new List<Action>();
                    var orderedIds = new uint[totalSprites];
                    int spriteIndex = 0;

                    // If we have a bitmap, prepare for extraction
                    IntPtr pixelsPtr = IntPtr.Zero;
                    int rowBytes = 0;
                    int bpp = 4;
                    int pixelsWidth = 0;
                    int pixelsHeight = 0;
                    int totalX = 1;

                    if (bgraBitmap != null)
                    {
                        pixelsPtr = bgraBitmap.GetPixels();
                        rowBytes = bgraBitmap.RowBytes;
                        pixelsWidth = vm.Width * 32;
                        pixelsHeight = vm.Height * 32;
                        totalX = bgraBitmap.Width / pixelsWidth;
                        if (totalX == 0) totalX = 1;
                    }

                    for (int f = 0; f < vm.Frames; f++)
                    for (int z = 0; z < vm.PatternZ; z++)
                    for (int py = 0; py < vm.PatternY; py++)
                    for (int px = 0; px < vm.PatternX; px++)
                    for (int l = 0; l < vm.Layers; l++)
                    {
                        // Calculate linear index for the block (Frame/Pattern combination)
                        // Standard Tibia order: Frames -> Z -> Y -> X -> Layers
                        int blockIndex = (((f * vm.PatternZ + z) * vm.PatternY + py) * vm.PatternX + px) * vm.Layers + l;
                        
                        int fx = 0, fy = 0;
                        if (bgraBitmap != null)
                        {
                            fx = (blockIndex % totalX) * pixelsWidth;
                            fy = (blockIndex / totalX) * pixelsHeight;
                        }

                        for (int w = 0; w < vm.Width; w++)
                        for (int h = 0; h < vm.Height; h++)
                        {
                            int currentIndex = spriteIndex;
                            uint currentId = startId + (uint)currentIndex;
                            orderedIds[currentIndex] = currentId;
                            
                            // Capture loop variables
                            int cw = w, ch = h;
                            
                            tasks.Add(() => 
                            {
                                byte[] rawPixels = new byte[4096];
                                
                                if (bgraBitmap != null)
                                {
                                    // Calculate Sprite Position within Block (Inverted)
                                    int pX = (vm.Width - cw - 1) * 32;
                                    int pY = (vm.Height - ch - 1) * 32;
                                    
                                    int sheetX = fx + pX;
                                    int sheetY = fy + pY;
                                    
                                    if (sheetX + 32 <= bgraBitmap.Width && sheetY + 32 <= bgraBitmap.Height)
                                    {
                                        unsafe
                                        {
                                            byte* srcPtr = (byte*)pixelsPtr;
                                            for (int row = 0; row < 32; row++)
                                            {
                                                byte* srcRow = srcPtr + ((sheetY + row) * rowBytes) + (sheetX * bpp);
                                                fixed (byte* destPtr = &rawPixels[row * 32 * 4])
                                                {
                                                    Buffer.MemoryCopy(srcRow, destPtr, 128, 128);
                                                }
                                            }
                                        }
                                    }
                                    // Else: rawPixels remains empty (transparent)
                                }
                                // Else: rawPixels remains empty (transparent) for blank object creation

                                byte[] compressedData = SpriteCompressor.Compress(rawPixels, ClientFeatures.Transparency);
                                var newSprite = new Sprite(currentId, ClientFeatures.Transparency) 
                                { 
                                    CompressedPixels = compressedData, 
                                    Size = (ushort)compressedData.Length 
                                };
                                newSpritesMap[currentId] = newSprite;
                            });
                            
                            spriteIndex++;
                        }
                    }

                    Parallel.Invoke(tasks.ToArray());

                    if (bgraBitmap != null) bgraBitmap.Dispose();

                    foreach (var id in orderedIds)
                    {
                        if (newSpritesMap.TryGetValue(id, out var sprite))
                        {
                            _loadedSprFile.Sprites[id] = sprite;
                        }
                    }
                    
                    Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
                    {
                        uint newThingId = 0;
                        switch (vm.SelectedCategory)
                        {
                            case ThingCategory.Item: newThingId = _loadedDatFile.Items.Keys.Max() + 1; break;
                            case ThingCategory.Outfit: newThingId = _loadedDatFile.Outfits.Keys.Max() + 1; break;
                            case ThingCategory.Effect: newThingId = _loadedDatFile.Effects.Keys.Max() + 1; break;
                            case ThingCategory.Missile: newThingId = _loadedDatFile.Missiles.Keys.Max() + 1; break;
                        }

                        var newThing = new ThingType
                        {
                            Id = newThingId,
                            Category = vm.SelectedCategory
                        };
                        
                        var group = new FrameGroup();
                        group.SetDimensions(vm.Width, vm.Height, vm.Layers, vm.PatternX, vm.PatternY, vm.PatternZ, vm.Frames);
                        
                        int idx = 0;
                        for (int f = 0; f < group.Frames; f++)
                        for (int z = 0; z < group.PatternZ; z++)
                        for (int py = 0; py < group.PatternY; py++)
                        for (int px = 0; px < group.PatternX; px++)
                        for (int l = 0; l < group.Layers; l++)
                        for (int w = 0; w < group.Width; w++)
                        for (int h = 0; h < group.Height; h++)
                        {
                            if (idx < orderedIds.Length)
                            {
                                group.SetSpriteId(f, px, py, z, l, w, h, orderedIds[idx++]);
                            }
                        }
                        
                        newThing.FrameGroups[FrameGroupType.Default] = group;
                        
                        switch (vm.SelectedCategory)
                        {
                            case ThingCategory.Item: _loadedDatFile.Items[newThing.Id] = newThing; break;
                            case ThingCategory.Outfit: _loadedDatFile.Outfits[newThing.Id] = newThing; break;
                            case ThingCategory.Effect: _loadedDatFile.Effects[newThing.Id] = newThing; break;
                            case ThingCategory.Missile: _loadedDatFile.Missiles[newThing.Id] = newThing; break;
                        }

                        StatusText = $"Created new object ID {newThing.Id} with {orderedIds.Length} sprites.";
                        SpriteCount = _loadedSprFile.SpriteCount;
                        
                        SelectedCategoryIndex = (int)vm.SelectedCategory;
                        RefreshList();
                        SelectedItem = newThing;
                        IsLoading = false;
                    });
                });
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Import Failed", ex.Message);
                Avalonia.Threading.Dispatcher.UIThread.Post(() => IsLoading = false);
            }
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
                LoadEditorPage(1);
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
                IsDirty = false; // Reset dirty state after saving
                StatusText = $"Item {SelectedItem.Id} saved to memory.";
                
                // Re-clone to keep TempProps in sync with the new state of SelectedItem
                TempProps = SelectedItem.Clone();
                UpdateComposerSlots();
            }
        }

        [RelayCommand]
        private void ChangeSpritePage(string direction)
        {
            int newPage = CurrentSpritePage;
            if (direction == "next" && CurrentSpritePage < TotalSpritePages) newPage++;
            else if (direction == "prev" && CurrentSpritePage > 1) newPage--;
            else if (direction == "next100" && CurrentSpritePage + 100 <= TotalSpritePages) newPage += 100;
            else if (direction == "prev100" && CurrentSpritePage - 100 >= 1) newPage -= 100;
            else if (direction == "first") newPage = 1;
            else if (direction == "last") newPage = TotalSpritePages;

            if (newPage != CurrentSpritePage)
            {
                CurrentSpritePage = newPage;
                LoadSpritePage(CurrentSpritePage);
            }
        }
        
        [RelayCommand]
        private void ChangeEditorPage(string direction)
        {
            int newPage = EditorCurrentPage;
            if (direction == "next" && EditorCurrentPage < EditorTotalPages) newPage++;
            else if (direction == "prev" && EditorCurrentPage > 1) newPage--;
            else if (direction == "next100" && EditorCurrentPage + 100 <= EditorTotalPages) newPage += 100;
            else if (direction == "prev100" && EditorCurrentPage - 100 >= 1) newPage -= 100;
            else if (direction == "first") newPage = 1;
            else if (direction == "last") newPage = EditorTotalPages;

            if (newPage != EditorCurrentPage)
            {
                EditorCurrentPage = newPage;
                LoadEditorPage(EditorCurrentPage);
            }
        }
        
        [RelayCommand]
        private void ChangeMainViewPage(string direction)
        {
            int newPage = MainViewCurrentPage;
            if (direction == "next" && MainViewCurrentPage < MainViewTotalPages) newPage++;
            else if (direction == "prev" && MainViewCurrentPage > 1) newPage--;
            else if (direction == "next100" && MainViewCurrentPage + 100 <= MainViewTotalPages) newPage += 100;
            else if (direction == "prev100" && MainViewCurrentPage - 100 >= 1) newPage -= 100;
            else if (direction == "first") newPage = 1;
            else if (direction == "last") newPage = MainViewTotalPages;

            if (newPage != MainViewCurrentPage)
            {
                MainViewCurrentPage = newPage;
                LoadMainViewPage(MainViewCurrentPage);
            }
        }

        partial void OnSelectedCategoryIndexChanged(int value)
        {
            RefreshList();
            if (IsEditing) LoadEditorPage(1);
        }

        private async Task LoadDataInternal(string datPath, string sprPath, string version)
        {
            // This method is now a legacy wrapper
            await LoadProject(datPath, sprPath, version, null, false);
        }

        private void LoadSpritePage(int page)
        {
            SpriteIds.Clear();
            CurrentSpritePage = page;
            uint startId = (uint)((page - 1) * SpritesPerPage) + 1;
            var endId = Math.Min((uint)page * SpritesPerPage, (uint)SpriteCount);
            for (uint i = startId; i <= endId; i++) SpriteIds.Add(i);
        }

        private void LoadEditorPage(int page)
        {
            EditorItems.Clear();
            var sourceDict = GetCurrentCategoryDictionary();
            if (sourceDict == null) return;

            EditorTotalPages = (int)Math.Ceiling((double)sourceDict.Count / EditorItemsPerPage);
            EditorCurrentPage = page;

            var pageItems = sourceDict.Values.Skip((page - 1) * EditorItemsPerPage).Take(EditorItemsPerPage);
            foreach (var thing in pageItems)
            {
                EditorItems.Add(thing);
            }
        }
        
        private void LoadMainViewPage(int page)
        {
            MainViewItems.Clear();
            
            MainViewTotalPages = (int)Math.Ceiling((double)AllItems.Count / MainViewItemsPerPage);
            MainViewCurrentPage = page;

            var pageItems = AllItems.Skip((page - 1) * MainViewItemsPerPage).Take(MainViewItemsPerPage);
            foreach (var thing in pageItems)
            {
                MainViewItems.Add(thing);
            }
        }

        private void RefreshList()
        {
            AllItems.Clear();
            var sourceDict = GetCurrentCategoryDictionary();
            if (sourceDict == null) return;
            foreach (var pair in sourceDict)
            {
                var thing = pair.Value;
                thing.Id = pair.Key;
                AllItems.Add(thing);
            }
            LoadMainViewPage(1);
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
