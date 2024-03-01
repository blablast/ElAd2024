using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Devices;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Helpers.General;
using ElAd2024.Models;
using Microsoft.Extensions.Options;
using Windows.Storage;

namespace ElAd2024.Services;

public partial class LocalSettingsService : ObservableRecipient, ILocalSettingsService
{
    #region Constructor

    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
        this.fileService = fileService;
        this.options = options.Value;

        // Initialize paths for application data and settings file
        applicationDataFolder = Path.Combine(localApplicationData,
            this.options.ApplicationDataFolder ?? DefaultApplicationDataFolder);
        localSettingsFile = this.options.LocalSettingsFile ?? DefaultLocalSettingsFile;

        settings = new Dictionary<string, object>();
    }

    #endregion Constructor

    #region Initialization

    // Initializes the service by loading settings from the local settings file or ApplicationData
    public async Task InitializeAsync()
    {
        isInitialized = true;
        settings = await Task.Run(() => fileService.Read<Dictionary<string, object>>(applicationDataFolder, localSettingsFile)) ?? [];

        // Load settings for each device and other configuration
        EnvDeviceSettings = await ReadSettingAsync<SerialPortInfo>(nameof(EnvDeviceSettings)) ?? new SerialPortInfo();
        ScaleDeviceSettings = await ReadSettingAsync<SerialPortInfo>(nameof(ScaleDeviceSettings)) ?? new SerialPortInfo();
        PadDeviceSettings = await ReadSettingAsync<SerialPortInfo>(nameof(PadDeviceSettings)) ?? new SerialPortInfo();
       
        RobotGotoPositionRegister = await ReadSettingAsync<int>(nameof(RobotGotoPositionRegister));
        RobotInPositionRegister = await ReadSettingAsync<int>(nameof(RobotInPositionRegister));
        RobotLoadForceRegister = await ReadSettingAsync<int>(nameof(RobotLoadForceRegister));
        RobotIsTouchSkipRegister = await ReadSettingAsync<int>(nameof(RobotIsTouchSkipRegister));
        RobotRunRegister = await ReadSettingAsync<int>(nameof(RobotRunRegister));
        RobotIpAddress = await ReadSettingAsync<string>(nameof(RobotIpAddress)) ?? string.Empty;
        Parameters = await ReadSettingAsync<TestParameters>(nameof(Parameters)) ?? new TestParameters();
        Simulate = await ReadSettingAsync<bool>(nameof(Simulate));
    }

    #endregion Initialization

    #region IDisposable Implementation

    public static void Dispose()
    {
        // Dispose logic if necessary
    }

    #endregion IDisposable Implementation

    #region Fields and Constants

    private const string DefaultApplicationDataFolder = "ElAd2024/ApplicationData";
    private const string DefaultLocalSettingsFile = "LocalSettings.json";
    private readonly IFileService fileService;
    private readonly LocalSettingsOptions options;

    private readonly string localApplicationData =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private readonly string applicationDataFolder;
    private readonly string localSettingsFile;
    private Dictionary<string, object> settings;
    private bool isInitialized;

    #endregion Fields and Constants

    #region Properties

    [ObservableProperty] private SerialPortInfo? envDeviceSettings;
    [ObservableProperty] private SerialPortInfo? scaleDeviceSettings;
    [ObservableProperty] private SerialPortInfo? padDeviceSettings;

    [ObservableProperty] private int robotGotoPositionRegister = 1;
    [ObservableProperty] private int robotLoadForceRegister = 2;
    [ObservableProperty] private int robotInPositionRegister = 1;
    [ObservableProperty] private int robotRunRegister = 2;
    [ObservableProperty] private int robotIsTouchSkipRegister = 3;
    [ObservableProperty] private string? robotIpAddress;

    [ObservableProperty] private bool simulate;

    [ObservableProperty] private TestParameters parameters = new();

    #endregion Properties

    #region Settings Operations

    // Reads a setting value by key
    public async Task<T?> ReadSettingAsync<T>(string key)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("Service not initialized");
        }

        // Retrieve settings depending on the application packaging
        if (RuntimeHelper.IsMSIX)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
            {
                return await Json.ToObjectAsync<T>((string)obj);
            }
        }
        else if (settings.TryGetValue(key, out var obj))
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
            if (!isInitialized)
            {
                throw new InvalidOperationException("Service not initialized");
            }

            settings[key] = await Json.StringifyAsync(value);
            await Task.Run(() => fileService.Save(applicationDataFolder, localSettingsFile, settings));
        }
    }

    // Convenience method to save all serial port settings
    public async Task SaveSerialsAsync()
    {
        await SaveSettingAsync(nameof(EnvDeviceSettings), EnvDeviceSettings);
        await SaveSettingAsync(nameof(ScaleDeviceSettings), ScaleDeviceSettings);
        await SaveSettingAsync(nameof(PadDeviceSettings), PadDeviceSettings);
    }

    public async Task SaveParametersAsync() => await SaveSettingAsync(nameof(Parameters), Parameters);

    #endregion Settings Operations

    #region Helpers

    async partial void OnRobotInPositionRegisterChanged(int value) =>
        await SaveSettingAsync(nameof(RobotInPositionRegister), value);
    async partial void OnRobotRunRegisterChanged(int value) =>
        await SaveSettingAsync(nameof(RobotRunRegister), value);
    async partial void OnRobotIsTouchSkipRegisterChanged(int value) =>
        await SaveSettingAsync(nameof(RobotIsTouchSkipRegister), value);
    async partial void OnRobotGotoPositionRegisterChanged(int value)
    => await SaveSettingAsync(nameof(RobotGotoPositionRegister), value);
    async partial void OnRobotLoadForceRegisterChanged(int value) =>
        await SaveSettingAsync(nameof(RobotLoadForceRegister), value);


    async partial void OnSimulateChanged(bool value) => await SaveSettingAsync(nameof(Simulate), value);

    // Validates the provided IP address using a regular expression
    [GeneratedRegex("^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")]
    private static partial Regex IpRegex();

    // Invoked when the RobotIpAddress property changes, validates the new value
    async partial void OnRobotIpAddressChanged(string? oldValue, string? newValue)
    {
        newValue ??= string.Empty;
        if (newValue.Length == 0 || IpRegex().IsMatch(newValue))
        {
            await SaveSettingAsync(nameof(RobotIpAddress), newValue);
            if (oldValue != newValue)
            {
                // Conncect to the robot
                await App.GetService<IAllDevices>().RobotDevice.ConnectAsync(newValue);
            }
        }
        else
        {
            RobotIpAddress = string.Empty;
        }
    }

    [RelayCommand]
    public async Task ResetAsync()
    {
        await Task.CompletedTask;
        //fileService.Delete(applicationDataFolder, localSettingsFile));
    }


    #endregion Helpers
}