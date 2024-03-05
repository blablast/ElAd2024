using ElAd2024.Views.UserControls;
using Microsoft.UI.Xaml.Controls;
namespace ElAd2024.Views.Dialogs;
public static class Dialogs
{
    public static async Task<bool> ShowYesNoQuestionAsync(string title, string content)
    => (await new YesNoDialog() { TitleText = title, ContentText = content }.ShowAsync()) == ContentDialogResult.Primary;

    public static async Task ShowInfoAsync(string title, string content)
        => await new InfoDialog() { TitleText = title, ContentText = content }.ShowAsync();
}
