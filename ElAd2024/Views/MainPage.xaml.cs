using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Models.Database;
using ElAd2024.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Telerik.UI.Xaml.Controls.Primitives;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace ElAd2024.Views;

[ObservableObject]
public sealed partial class MainPage : Page
{
    private RadSideDrawer radSideDrawer = new();
    [ObservableProperty] private IMediaPlaybackSource? cameraPlayback;
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
            ExpanderPlot.IsExpanded = ViewModel.IsTesting;
        }
    }
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
        ArgumentNullException.ThrowIfNull(ViewModel.AllDevices.PadDevice);
        ViewModel.AllDevices.PadDevice.Voltages.CollectionChanged += OnVoltagesChartDataCollectionChanged;
        if (ViewModel.AllDevices.MediaDevice.SelectedMediaFrameSourceGroup is not null)
        {
            CameraPlayback = MediaSource.CreateFromMediaFrameSource(
             ViewModel.AllDevices.MediaDevice.MediaCapture?.FrameSources[ViewModel.AllDevices.MediaDevice.SelectedMediaFrameSourceGroup?.SourceInfos[0].Id]);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AllDevices.PadDevice is not null)
        {
            ViewModel.AllDevices.PadDevice.Voltages.CollectionChanged -= OnVoltagesChartDataCollectionChanged;
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
    private void OnVoltagesChartDataCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs? e = null)
    {
        if (e?.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            var voltages = sender is null ? [] : (ObservableCollection<Voltage>)sender;
            voltagesChartSerie4.ItemsSource = voltages;
            voltagesChartSerie3.ItemsSource = voltages.Where(v => v.Phase < 4);
            voltagesChartSerie2.ItemsSource = voltages.Where(v => v.Phase < 3);
            voltagesChartSerie1.ItemsSource = voltages.Where(v => v.Phase < 2);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (ViewModel.AllDevices.PadDevice is not null)
        {
            ViewModel.AllDevices.PadDevice.Voltages.CollectionChanged -= OnVoltagesChartDataCollectionChanged;
        }
        ViewModel.Dispose();
        base.OnNavigatedFrom(e);
    }
}
