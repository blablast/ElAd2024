using ElAd2024.Models;

namespace ElAd2024.Contracts.Services;

public interface ILocalSettingsService
{

    SerialPortInfo EnvDeviceSettings { get; set; }
    SerialPortInfo ScaleDeviceSettings { get; set; }
    SerialPortInfo PadDeviceSettings { get; set; }
    public int RobotGotoPositionRegister  { get; set; }
    public int RobotInPositionRegister { get; set; }
    public int RobotLoadForceRegister { get; set; }
    public string RobotIPAddress { get; set; }

    Task InitializeAsync();
    Task<T?> ReadSettingAsync<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
    Task SaveSerialsAsync();
}
