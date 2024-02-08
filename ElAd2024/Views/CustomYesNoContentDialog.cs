using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media.Imaging;
namespace ElAd2024.Views;
public static class CustomContentDialog
{
    public static async Task<bool> ShowYesNoQuestionAsync(XamlRoot? xamlRoot, string title, string content)
    {
        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch };

        stackPanel.Children.Add(new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center });
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"  \uE9CE",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            HorizontalAlignment = HorizontalAlignment.Right,
            FontSize = 24,
            Foreground = new SolidColorBrush(Colors.LightSkyBlue),
            Margin = new Thickness(0, 0, 10, 0)
        });
       
        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = stackPanel,
            Content = content,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            DefaultButton = ContentDialogButton.Primary
        };
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public static async Task ShowInfoAsync(XamlRoot? xamlRoot, string title, string content)
    {
        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch };

        stackPanel.Children.Add(new TextBlock { Text = title, VerticalAlignment = VerticalAlignment.Center });
        stackPanel.Children.Add(new TextBlock
        {
            Text = $"\uE946",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            HorizontalAlignment = HorizontalAlignment.Right,
            FontSize = 24,
            Foreground = new SolidColorBrush(Colors.LightSkyBlue),
            Margin = new Thickness(0, 0, 10, 0)
        });

        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = stackPanel,
            Content = content,
            CloseButtonText = "Ok",
            DefaultButton = ContentDialogButton.Close
        };
        await dialog.ShowAsync();
    }

    public static async Task ShowImageAsync(XamlRoot? xamlRoot, BitmapImage source)
    {
        
        var stackPanel = new StackPanel {HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        stackPanel.Children.Add(new Image
        {
            Source = source,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        });

        var dialog = new ContentDialog
        {
            FullSizeDesired = true,
            XamlRoot = xamlRoot,
            Title = string.Empty,
            Content = stackPanel,
            CloseButtonText = "Ok",
            MinHeight = 800,
            MinWidth = 1200,
            DefaultButton = ContentDialogButton.Close
        };
        await dialog.ShowAsync();
    }
}
