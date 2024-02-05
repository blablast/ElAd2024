using System.Collections.ObjectModel;
using ElAd2024.Core.Models;
using ElAd2024.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ElAd2024.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync(XamlRoot);
        ViewModel.PadDevice.ChartDataCollection.CollectionChanged += OnChartDataCollectionChanged;
        OnChartDataCollectionChanged(ViewModel.PadDevice.ChartDataCollection);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ViewModel.PadDevice.ChartDataCollection.CollectionChanged -= OnChartDataCollectionChanged;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        Loaded -= OnLoaded;
    }

    // Chart

    private void OnChartDataCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs? e = null)
    {
        if (sender is ObservableCollection<HVPlot> collection
            && (e is null || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add))
        {
            myChartSerie1.ItemsSource = collection;
            myChartSerie2.ItemsSource = collection;
            myChartSerie3.ItemsSource = collection;
            myChartSerie4.ItemsSource = collection;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        try
        {
            ViewModel.PadDevice.ChartDataCollection.CollectionChanged -= OnChartDataCollectionChanged;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

        ViewModel.Dispose();
        base.OnNavigatedFrom(e);
    }

}
