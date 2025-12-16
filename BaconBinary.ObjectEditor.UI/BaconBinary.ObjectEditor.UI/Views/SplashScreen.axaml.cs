using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using BaconBinary.Core;
using BaconBinary.ObjectEditor.UI.Services;
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

            await Task.WhenAll(delayTask, loadTask);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (loadingException != null)
                {
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
                    }
                }
            });
        }
    }
}
