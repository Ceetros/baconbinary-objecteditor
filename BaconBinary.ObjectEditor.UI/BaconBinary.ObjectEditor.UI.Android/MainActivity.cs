using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace BaconBinary.ObjectEditor.UI.Android;

[Activity(
    Label = "BaconBinary.ObjectEditor.UI.Android",
    Theme = "@style/MyTheme.Splash",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
