using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Models;
using Windows.Storage;

namespace ElAd2024.Contracts.Services;

public interface ILocalSettingsService
{
    string ApplicationDataFolder { get; }
    string LocalSettingsFile { get; }


    SerialPortInfo EnvDeviceSettings { get; set; }
    SerialPortInfo PadDeviceSettings { get; set; }
    SerialPortInfo ScaleDeviceSettings { get; set; }
    int RobotGotoPositionRegister  { get; set; }
    int RobotInPositionRegister { get; set; }
    string RobotIpAddress { get; set; }
    int RobotLoadForceRegister { get; set; }
    int RobotRunRegister { get; set; }
    bool Simulate { get; set; }
    TestParameters Parameters { get; set; }

    string PicturesFolder
    {
        get;
    }
    Task InitializeAsync();
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
    Task SaveSerialsAsync();
    Task SaveParametersAsync();

    Task ResetAsync();
}
