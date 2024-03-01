using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Store;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ElAd2024.Views;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
[ObservableObject]
public sealed partial class CustomSplashScreen : Page
{
    [ObservableProperty] private string progress = "Loading...";
    public CustomSplashScreen()
    {
        InitializeComponent();
    }
}
