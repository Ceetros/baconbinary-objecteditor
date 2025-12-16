using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using BaconBinary.ObjectEditor.UI.ViewModels;

namespace BaconBinary.ObjectEditor.UI.Views
{
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
            _ = LoadMainWindowAsync();
        }

        private async Task LoadMainWindowAsync()
        {
            // Start the delay
            var delayTask = Task.Delay(1000); // Minimum 1 second delay
            
            MainViewModel viewModel = null;
            Exception loadingException = null;

            // Run data loading in background
            var loadTask = Task.Run(() =>
            {
                try
                {
                    // Only initialize the ViewModel (logic/data) in the background thread.
                    // UI components (Window) must be created on the UI thread.
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
                    // Close splash screen so we don't have two windows if user closes error
                    this.Close(); 
                }
                else
                {
                    try 
                    {
                        // Create MainWindow on the UI Thread
                        var mainWindow = new MainWindow
                        {
                            DataContext = viewModel
                        };
                        mainWindow.Show();
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        // Handle errors that might happen during Window creation
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
