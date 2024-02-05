using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
namespace ElAd2024.Views;
public static class CustomContentDialog
{
    public static async Task<bool> ShowYesNoQuestionAsync(XamlRoot xamlRoot, string title, string content)
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
}
