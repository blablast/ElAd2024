using ElAd2024.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ElAd2024.Views;

public sealed partial class ManageBatchesPage : Page
{
    public ManageBatchesViewModel ViewModel
    {
        get;
    }

    public ManageBatchesPage()
    {
        ViewModel = App.GetService<ManageBatchesViewModel>();
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
        => await ViewModel.InitializeAsync(XamlRoot);

    private void OnUnloaded(object sender, RoutedEventArgs e)
        => Loaded -= OnLoaded;
}
