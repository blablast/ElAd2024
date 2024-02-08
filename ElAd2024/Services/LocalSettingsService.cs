using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Core.Contracts.Services;
using ElAd2024.Core.Helpers;
using ElAd2024.Helpers;
using ElAd2024.Models;
using Microsoft.Extensions.Options;
using Windows.Storage;

namespace ElAd2024.Services;

public partial class LocalSettingsService : ObservableRecipient, ILocalSettingsService
{
    #region Fields and Constants

    private const string defaultApplicationDataFolder = "ElAd2024/ApplicationData";
    private const string defaultLocalSettingsFile = "LocalSettings.json";
    private readonly IFileService fileService;
    private readonly LocalSettingsOptions options;
    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string applicationDataFolder;
    private readonly string localsettingsFile;
    private IDictionary<string, object> settings;
    private bool isInitialized;

    #endregion

    #region Properties

    [ObservableProperty] private SerialPortInfo? envDeviceSettings;
    [ObservableProperty] private SerialPortInfo? scaleDeviceSettings;
    [ObservableProperty] private SerialPortInfo? padDeviceSettings;

    [ObservableProperty] private int robotGotoPositionRegister;
    [ObservableProperty] private int robotInPositionRegister;
    [ObservableProperty] private int robotLoadForceRegister;
    [ObservableProperty] private string? robotIPAddress;

    #endregion

    #region Constructor

    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        this.fileService = fileService;
        this.options = options.Value;

        // Initialize paths for application data and settings file
        applicationDataFolder = Path.Combine(localApplicationData, this.options.ApplicationDataFolder ?? defaultApplicationDataFolder);
        localsettingsFile = this.options.LocalSettingsFile ?? defaultLocalSettingsFile;

        settings = new Dictionary<string, object>();
    }

    #endregion

    #region Initialization

    // Initializes the service by loading settings from the local settings file or ApplicationData
    public async Task InitializeAsync()
    {
        isInitialized = true;
        settings = await Task.Run(() => fileService.Read<IDictionary<string, object>>(applicationDataFolder, localsettingsFile)) ?? new Dictionary<string, object>();

        // Load settings for each device and other configuration
        EnvDeviceSettings = await ReadSettingAsync<SerialPortInfo>(nameof(EnvDeviceSettings)) ?? new SerialPortInfo();
        ScaleDeviceSettings = await ReadSettingAsync<SerialPortInfo>(nameof(ScaleDeviceSettings)) ?? new SerialPortInfo();
        PadDeviceSettings = await ReadSettingAsync<SerialPortInfo>(nameof(PadDeviceSettings)) ?? new SerialPortInfo();
        RobotGotoPositionRegister = await ReadSettingAsync<int>(nameof(RobotGotoPositionRegister));
        RobotInPositionRegister = await ReadSettingAsync<int>(nameof(RobotInPositionRegister));
        RobotLoadForceRegister = await ReadSettingAsync<int>(nameof(RobotLoadForceRegister));
        RobotIPAddress = await ReadSettingAsync<string>(nameof(RobotIPAddress)) ?? string.Empty;
    }

    #endregion

    #region Settings Operations

    // Reads a setting value by key
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        if (!isInitialized) { throw new InvalidOperationException("Service not initialized"); }

        // Retrieve settings depending on the application packaging
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else if (settings != null && settings.TryGetValue(key, out var obj))
        {
            return await Json.ToObjectAsync<T>((string)obj);
        }
        return default;
    }

    // Saves a setting value by key
    public async Task SaveSettingAsync<T>(string key, T value)
    {
        if (value is null)
        {
            return;
        }

        // Save settings depending on the application packaging
        if (RuntimeHelper.IsMSIX)
        {
            ApplicationData.Current.LocalSettings.Values[key] = await Json.StringifyAsync(value);
        }
        else
        {
            if (!isInitialized) { throw new InvalidOperationException("Service not initialized"); }
            settings[key] = await Json.StringifyAsync(value);
            await Task.Run(() => fileService.Save(applicationDataFolder, localsettingsFile, settings));
        }
    }

    // Convenience method to save all serial port settings
    public async Task SaveSerialsAsync()
    {
        await SaveSettingAsync(nameof(EnvDeviceSettings), EnvDeviceSettings);
        await SaveSettingAsync(nameof(ScaleDeviceSettings), ScaleDeviceSettings);
        await SaveSettingAsync(nameof(PadDeviceSettings), PadDeviceSettings);
    }

    #endregion

    #region Helpers

    // Validates the provided IP address using a regular expression
    [GeneratedRegex("^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
    private static partial Regex IPRegex();

    // Invoked when the RobotIPAddress property changes, validates the new value
    async partial void OnRobotIPAddressChanged(string? value)
    {
        value ??= string.Empty;
        if (value.Length == 0 || IPRegex().IsMatch(value))
        {
            await SaveSettingAsync(nameof(RobotIPAddress), value);
        }
        else
        {
            RobotIPAddress = string.Empty;
        }
    }

    // Invoked when RobotGotoPositionRegister property changes, saves the new value
    async partial void OnRobotGotoPositionRegisterChanged(int value)
        => await SaveSettingAsync(nameof(RobotGotoPositionRegister), value);

    // Similar partial methods for other properties...

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        // Dispose logic if necessary
    }

    #endregion
}
