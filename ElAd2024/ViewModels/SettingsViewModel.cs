using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.Services;
using Microsoft.UI.Xaml;

using Windows.ApplicationModel;

namespace ElAd2024.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;

    [ObservableProperty] private SerialPortInfo? environmentalSensorsSettings;
    [ObservableProperty] private SerialPortInfo? scaleDeviceSettings;
    [ObservableProperty] private SerialPortInfo? padDeviceSettings;

    [GeneratedRegex("^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
    private static partial Regex IPRegex();

    private string ipAddress = string.Empty;
    public string IPAddress
    {
        get => ipAddress;
        set
        {
            if (IPRegex().IsMatch(value))
            {
                ipAddress = value;
                _localSettingsService.SaveSettingAsync("IPAddress", IPAddress);
            }
            OnPropertyChanged();

        }
    }
    // Selected SerialPortInfo for each device
    [ObservableProperty] private string selectedEnvironmentalSensorsPort = string.Empty;
    partial void OnSelectedEnvironmentalSensorsPortChanged(string value)
        => AssignPortName(EnvironmentalSensorsSettings, value);

    [ObservableProperty] private string selectedScaleDevicePort = string.Empty;
    partial void OnSelectedScaleDevicePortChanged(string value)
        => AssignPortName(ScaleDeviceSettings, value);

    [ObservableProperty] private string selectedPadDevicePort = string.Empty;
    partial void OnSelectedPadDevicePortChanged(string value)
        => AssignPortName(PadDeviceSettings, value);

    private static void AssignPortName(SerialPortInfo? value, string selected)
    {
        if (value is not null)
        {
            value.PortName = selected?[..(selected.IndexOf(':'))];
            value.Name = selected?[(selected.IndexOf(':') + 2)..];
        }
    }


    [ObservableProperty] private ElementTheme _elementTheme;
    [ObservableProperty] private string _versionDescription;

    private readonly ILocalSettingsService _localSettingsService;

    public SettingsViewModel(ILocalSettingsService localSettingsService, IThemeSelectorService themeSelectorService)
    {
        _themeSelectorService = themeSelectorService;
        _localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();
        LoadAvailableSerialPortsAndInitializeSettings();
    }

    public ObservableCollection<string?> AvailableSerialPorts { get; set; } = [];
    private async void LoadAvailableSerialPortsAndInitializeSettings()
    {
        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailableSerialPorts.Add(port.ToString()));
        EnvironmentalSensorsSettings = await LoadSettingAsync("EnvironmentalSensorsSettings");
        SelectedEnvironmentalSensorsPort = EnvironmentalSensorsSettings.ToString();

        ScaleDeviceSettings = await LoadSettingAsync("ScaleDeviceSettings");
        SelectedScaleDevicePort = ScaleDeviceSettings.ToString();

        PadDeviceSettings = await LoadSettingAsync("PadDeviceSettings");
        SelectedPadDevicePort = PadDeviceSettings.ToString();

        IPAddress = await _localSettingsService.ReadSettingAsync<string>("IPAddress") ?? string.Empty;

        async Task<SerialPortInfo> LoadSettingAsync(string key)
        {
            var setting = await _localSettingsService.ReadSettingAsync<SerialPortInfo>(key);
            return setting ?? new SerialPortInfo();
        }
    }

    private static string GetVersionDescription()
    {
        Version version;
        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;
            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }
        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    [RelayCommand]
    public async Task SwitchThemeAsync(ElementTheme param)
    {
        if (ElementTheme != param)
        {
            ElementTheme = param;
            await _themeSelectorService.SetThemeAsync(param);
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _localSettingsService.SaveSettingAsync("IPAddress", IPAddress);
        _localSettingsService.SaveSettingAsync("EnvironmentalSensorsSettings", EnvironmentalSensorsSettings);
        _localSettingsService.SaveSettingAsync("ScaleDeviceSettings", ScaleDeviceSettings);
        _localSettingsService.SaveSettingAsync("PadDeviceSettings", PadDeviceSettings);
    }

}
