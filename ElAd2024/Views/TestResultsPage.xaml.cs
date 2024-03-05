using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Telerik.UI.Xaml.Controls.Primitives;

namespace ElAd2024.Views;

[ObservableObject]
public sealed partial class TestResultsPage : Page
{
    private RadSideDrawer radSideDrawer = new();
    public TestResultsViewModel ViewModel { get; }

    public TestResultsPage()
    {
        ViewModel = App.GetService<TestResultsViewModel>();
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

    }
    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    private void OnUnloaded(object sender, RoutedEventArgs e) => ViewModel.PropertyChanged -= ViewModel_PropertyChanged;


    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Selected)) { radSideDrawer.IsOpen = false; }

    }
    private void Image_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        var imageElement = sender as Microsoft.UI.Xaml.Controls.Image;
        if (imageElement is not null)
        {
            PopupImage.Source = imageElement.Source;
            StandardPopup.Width = MainContent.ActualWidth;
            StandardPopup.Height = MainContent.ActualHeight;
            StandardPopup.IsOpen = true;
        }
    }

    private void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        DataGridView.Visibility = (DataGridView.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        ChartsView.Visibility = (ChartsView.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
        radSideDrawer.IsOpen = false;
    }

    private void RadSideDrawer_DrawerOpening(object sender, EventArgs e)
    {
        StandardPopup.IsOpen = false;
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

    private void ClosePopupClicked(object sender, RoutedEventArgs e) => StandardPopup.IsOpen = false;
}
