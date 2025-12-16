using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using Avalonia.Controls;
using BaconBinary.ObjectEditor.UI.ViewModels;
using BaconBinary.ObjectEditor.UI.Views;

namespace BaconBinary.ObjectEditor.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Always start with the SplashScreen
            desktop.MainWindow = new SplashScreen();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            try 
            {
                singleView.MainView = new MainView
                {
                    DataContext = new MainViewModel()
                };
            }
            catch (Exception ex)
            {
                singleView.MainView = new UserControl
                {
                    Content = new ScrollViewer()
                    {
                        Content = new TextBlock 
                        { 
                            Text = $"Critical Error during startup:\n\n{ex}", 
                            Margin = new Thickness(20),
                            Foreground = Avalonia.Media.Brushes.Red,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        }
                    }
                };
            }
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}
