using System.Collections.ObjectModel;
using ElAd2024.Core.Models;
using ElAd2024.Services;
using ElAd2024.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Telerik.UI.Xaml;
using Telerik.UI.Xaml.Controls.Primitives;

namespace ElAd2024.Views;

public sealed partial class MainPage : Page
{
    private RadSideDrawer radSideDrawer = new();
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
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Selected))
        {
            radSideDrawer.IsOpen = false;
        }
        else if (e.PropertyName == nameof(ViewModel.IsTesting))
        {
            ExpanderSettings.IsExpanded = !ViewModel.IsTesting;
            ExpanderPlot.IsExpanded = ViewModel.IsTesting;
        }
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

    private void RadSideDrawer_DrawerOpening(object sender, EventArgs e)
    {
        MainContent.Margin = new Thickness(224, 0, 0, 0); // Push content to the right
    }

    private void RadSideDrawer_DrawerClosing(object sender, EventArgs e)
    {
        MainContent.Margin = new Thickness(48, 0, 0, 0); // Reset margin
    }

    private void RadSideDrawer_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not null)
        {
            radSideDrawer = (RadSideDrawer)sender;
            radSideDrawer.IsOpen = true;
        }
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
