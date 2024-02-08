using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.Services;

namespace ElAd2024.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    #region Fields
    private readonly IThemeSelectorService themeSelectorService;
    #endregion

    #region Properties
    public ILocalSettingsService LocalSettingsService { get; set; }

    [ObservableProperty] private string selectedEnvDevicePort = string.Empty;
    partial void OnSelectedEnvDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.EnvDeviceSettings, value);

    [ObservableProperty] private string selectedScaleDevicePort = string.Empty;
    partial void OnSelectedScaleDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.ScaleDeviceSettings, value);

    [ObservableProperty] private string selectedPadDevicePort = string.Empty;
    partial void OnSelectedPadDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.PadDeviceSettings, value);

    [ObservableProperty] private Microsoft.UI.Xaml.ElementTheme elementTheme;
    [ObservableProperty] private string versionDescription;
    public ObservableCollection<string?> AvailableSerialPorts { get; set; } = [];
    #endregion

    #region Constructors
    public SettingsViewModel(ILocalSettingsService localSettingsService, IThemeSelectorService themeSelectorService)
    {
        this.themeSelectorService = themeSelectorService;
        LocalSettingsService = localSettingsService;
        ElementTheme = this.themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();
        LoadAvailableSerialPortsAndInitializeSettings();
    }

    #endregion

    #region Private Methods
    private static void AssignPortName(SerialPortInfo? value, string selected)
    {
        if (value is not null)
        {
            value.PortName = selected?[..(selected.IndexOf(':'))];
            value.Name = selected?[(selected.IndexOf(':') + 2)..];
        }
    }
    private async void LoadAvailableSerialPortsAndInitializeSettings()
    {
        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailableSerialPorts.Add(port.ToString()));
        SelectedEnvDevicePort = LocalSettingsService.EnvDeviceSettings.ToString();
        SelectedScaleDevicePort = LocalSettingsService.ScaleDeviceSettings.ToString();
        SelectedPadDevicePort = LocalSettingsService.PadDeviceSettings.ToString();
    }

    private static string GetVersionDescription()
    {
        Version version;
        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
        }
        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    #endregion

    #region Commands

    [RelayCommand]
    public async Task SwitchThemeAsync(Microsoft.UI.Xaml.ElementTheme param)
    {
        if (ElementTheme != param)
        {
            ElementTheme = param;
            await themeSelectorService.SetThemeAsync(param);
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
        => await LocalSettingsService.SaveSerialsAsync();

    #endregion

}
