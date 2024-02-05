using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;

using Microsoft.UI.Xaml;

namespace ElAd2024.Services;

public class ThemeSelectorService(ILocalSettingsService localSettingsService) : IThemeSelectorService
{
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    private readonly ILocalSettingsService localSettingsService = localSettingsService;

    public async Task InitializeAsync()
    {
        Theme = await LoadThemeFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
        Theme = theme;
        await SetRequestedThemeAsync();
        await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;
            TitleBarHelper.UpdateTitleBar(Theme);
        }
        await Task.CompletedTask;
    }

    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
        => (Enum.TryParse(await localSettingsService.ReadSettingAsync<string>(SettingsKey), out ElementTheme cacheTheme)) ? cacheTheme : ElementTheme.Default;

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
        => await localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
}
