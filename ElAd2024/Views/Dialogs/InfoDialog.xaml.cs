using Microsoft.UI.Xaml.Controls;

namespace ElAd2024.Views.Dialogs;

public sealed partial class InfoDialog : ContentDialog
{
    public string TitleText { get; set; } = string.Empty;
    public string ContentText { get; set; } = string.Empty;

    public InfoDialog()
    {
        InitializeComponent();
        DataContext = this;
        XamlRoot ??= App.MainWindow.Content.XamlRoot;
    }
}