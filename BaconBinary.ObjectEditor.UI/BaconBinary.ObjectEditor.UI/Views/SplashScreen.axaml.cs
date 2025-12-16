using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using BaconBinary.Core;
using BaconBinary.ObjectEditor.UI.ViewModels;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class SplashScreen : Window
    {
        public string AppVersion => Definitions.Version;

        public SplashScreen()
        {
            InitializeComponent();
            DataContext = this;
            _ = LoadMainWindowAsync();
        }

        private async Task LoadMainWindowAsync()
        {
            var delayTask = Task.Delay(1000);
            
            MainViewModel viewModel = null;
            Exception loadingException = null;

            // Run data loading in background
            var loadTask = Task.Run(() =>
            {
                try
                {
                    viewModel = new MainViewModel();
                }
                catch (Exception ex)
                {
                    loadingException = ex;
                }
            });

            // Wait for both tasks to complete
            await Task.WhenAll(delayTask, loadTask);

            // Switch back to UI thread to create the Window and show it
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (loadingException != null)
                {
                    var errorDialog = new Window
                    {
                        Title = "Error",
                        Content = new TextBlock 
                        { 
                            Text = $"Failed to load the application.\n\n{loadingException.Message}", 
                            Margin = new Avalonia.Thickness(20),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        Width = 400,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };
                    errorDialog.Show();
                    this.Close(); 
                }
                else
                {
                    try 
                    {
                        var mainWindow = new MainWindow
                        {
                            DataContext = viewModel
                        };
                        mainWindow.Show();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                         var errorDialog = new Window
                        {
                            Title = "UI Error",
                            Content = new TextBlock 
                            { 
                                Text = $"Failed to create main window.\n\n{ex.Message}", 
                                Margin = new Avalonia.Thickness(20) 
                            },
                            Width = 400,
                            Height = 200,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        errorDialog.Show();
                        this.Close();
                    }
                }
            });
        }
    }
}
