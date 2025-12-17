using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BaconBinary.Core.IO.Otb;
using BaconBinary.Core.Models;
using BaconBinary.ObjectEditor.UI.Views;

namespace BaconBinary.ObjectEditor.UI.ViewModels
{
    public partial class ItemEditorViewModel : ObservableObject
    {
        private readonly OtbReader _otbReader = new();
        private readonly ItemsXmlReader _itemsXmlReader = new();
        
        private string _otbPath;
        private string _xmlPath;

        public ObservableCollection<ServerItem> Items { get; } = new();
        
        [ObservableProperty] private ServerItemList _serverItems;
        [ObservableProperty] private ServerItem _selectedItem;
        
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private int _itemsPerPage = 200;
        public List<int> ItemsPerPageOptions { get; } = new() { 100, 200, 500, 1000 };

        public ItemEditorViewModel(ServerItemList serverItems, string otbPath, string xmlPath)
        {
            _serverItems = serverItems;
            _otbPath = otbPath;
            _xmlPath = xmlPath;
            
            LoadPage(1);
        }

        public void LoadFiles(string datPath)
        {
            _otbPath = Path.ChangeExtension(datPath, ".otb");
            _xmlPath = Path.ChangeExtension(datPath, ".xml");

            if (File.Exists(_otbPath))
            {
                _serverItems = _otbReader.Read(_otbPath);
                if (File.Exists(_xmlPath))
                {
                    _itemsXmlReader.Read(_xmlPath, _serverItems);
                }
                LoadPage(1);
            }
        }
        
        private void LoadPage(int page)
        {
            Items.Clear();
            if (_serverItems == null) return;

            TotalPages = (int)Math.Ceiling((double)_serverItems.Count / ItemsPerPage);
            CurrentPage = page;

            var pageItems = _serverItems.Values.OrderBy(i => i.Id).Skip((page - 1) * ItemsPerPage).Take(ItemsPerPage);
            foreach (var item in pageItems)
            {
                Items.Add(item);
            }
        }

        [RelayCommand]
        private void ChangePage(string direction)
        {
            int newPage = CurrentPage;
            if (direction == "next" && CurrentPage < TotalPages) newPage++;
            else if (direction == "prev" && CurrentPage > 1) newPage--;
            else if (direction == "first") newPage = 1;
            else if (direction == "last") newPage = TotalPages;

            if (newPage != CurrentPage)
            {
                LoadPage(newPage);
            }
        }

        public void CreateNewItem(uint id)
        {
            if (_serverItems.ContainsKey(id)) return;

            var newItem = new ServerItem { Id = (ushort)id };
            _serverItems[id] = newItem;
            LoadPage(TotalPages); // Go to last page
            SelectedItem = newItem;
        }

        public void GoToItem(uint id)
        {
            if (_serverItems.TryGetValue(id, out var item))
            {
                var index = _serverItems.Values.OrderBy(i => i.Id).ToList().IndexOf(item);
                var page = (index / ItemsPerPage) + 1;
                LoadPage(page);
                SelectedItem = item;
            }
        }

        [RelayCommand]
        private async Task SaveChanges()
        {
            if (_serverItems == null) return;

            try
            {
                new OtbWriter(_serverItems).Write(_otbPath);
                new ItemsXmlWriter(_serverItems).Write(_xmlPath);
                await ShowErrorDialog("Success", "Files saved successfully.");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Error", $"Failed to save files: {ex.Message}");
            }
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var vm = new MessageBoxViewModel(title, message);
                var dialog = new MessageBoxWindow { DataContext = vm };
                
                if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var owner = desktop.Windows.FirstOrDefault(w => w.IsActive && w.IsVisible);
                    
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
    }
}
