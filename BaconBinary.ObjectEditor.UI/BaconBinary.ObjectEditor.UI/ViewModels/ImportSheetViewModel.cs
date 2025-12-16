using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using BaconBinary.Core.Enum;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class ImportSheetViewModel : ObservableObject
    {
        [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsFileSelected))] private string _filePath;
        [ObservableProperty] private ThingCategory _selectedCategory = ThingCategory.Item;
        [ObservableProperty] private byte _width = 1;
        [ObservableProperty] private byte _height = 1;
        [ObservableProperty] private byte _layers = 1;
        [ObservableProperty] private byte _patternX = 1;
        [ObservableProperty] private byte _patternY = 1;
        [ObservableProperty] private byte _patternZ = 1;
        [ObservableProperty] private byte _frames = 1;
        
        public bool IsFileSelected => !string.IsNullOrEmpty(FilePath);
        
        public IEnumerable<ThingCategory> Categories => Enum.GetValues<ThingCategory>();
        public bool Success { get; private set; } = false;

        public ImportSheetViewModel(string filePath = null)
        {
            FilePath = filePath;
        }

        [RelayCommand]
        private async Task BrowseFile(Window window)
        {
            var topLevel = TopLevel.GetTopLevel(window);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Sprite Sheet",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count > 0)
            {
                FilePath = files[0].Path.LocalPath;
            }
        }

        [RelayCommand]
        private void Confirm(Window window)
        {
            if (!IsFileSelected) return;
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
