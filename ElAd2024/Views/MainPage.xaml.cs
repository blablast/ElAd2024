using System.Collections.ObjectModel;
using ElAd2024.Core.Models;
using ElAd2024.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ElAd2024.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        ViewModel.TemperatureHumidityChartDataCollection.CollectionChanged += OnChartDataCollectionChanged;
    }

    private void OnChartDataCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add 
            && sender is ObservableCollection<TemperatureHumidityChartData> collection)
        {
            myChartSerie1.ItemsSource = collection;
            myChartSerie2.ItemsSource = collection;
        }

    }
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.TemperatureHumidityChartDataCollection.CollectionChanged -= OnChartDataCollectionChanged;
        ViewModel.Dispose();
        base.OnNavigatedFrom(e);
    }
}
