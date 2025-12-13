using CommunityToolkit.Mvvm.ComponentModel;

namespace BaconBinary.ObjectEditor.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
