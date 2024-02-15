using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using ElAd2024.Contracts.Services;
using ElAd2024.Devices.Serial;
using ElAd2024.Devices.Simulator;
using ElAd2024.Models;
using ElAd2024.Services;

namespace ElAd2024.Devices;
public partial class AllDevices(ILocalSettingsService localSettingsService) : ObservableObject, IAllDevices
{
    private readonly ILocalSettingsService localSettingsService = localSettingsService;
    [ObservableProperty] private IPadDevice padDevice = new PadDevice();
    [ObservableProperty] private ICameraDevice cameraDevice = new CameraDevice();
    [ObservableProperty] private IHumidityDevice humidityDevice = new HumidityAndTemperatureDevice();
    [ObservableProperty] private IRobotDevice robotDevice = new RobotDevice();
    [ObservableProperty] private IScaleDevice scaleDevice = new ScaleDevice();
    [ObservableProperty] private ITemperatureDevice temperatureDevice = new HumidityAndTemperatureDevice();

    public ObservableCollection<SerialPortInfo> AvailablePorts { get; } = [];
    public ObservableCollection<string> AvailablePortsNames { get; } = [];

    public async Task InitializePortsAsync()
    {
        AvailablePorts.Clear();
        AvailablePortsNames.Clear();
        var allPorts = await SerialPortManagerService.GetAvailableSerialPortsAsync();
        foreach (var port in allPorts)
        {
            AvailablePorts.Add(port);
            AvailablePortsNames.Add(port.ToString());
        }
    }

    public async Task InitializeTemperatureAndHumidityAsync()
    {
        await TemperatureDevice.ConnectAsync(LoadSpi(localSettingsService.EnvDeviceSettings));
        if (TemperatureDevice.IsConnected && localSettingsService.Simulate)
        {
            HumidityDevice = (HumidityAndTemperatureDevice)TemperatureDevice;
        }
        else if (localSettingsService.Simulate)
        {
            TemperatureDevice = new HumidityAndTemperatureSimulator();
            await TemperatureDevice.ConnectAsync();
            HumidityDevice = (HumidityAndTemperatureSimulator)TemperatureDevice;
        }
    }
    public async Task InitializeScaleAsync()
    {
        await ScaleDevice.ConnectAsync(LoadSpi(localSettingsService.ScaleDeviceSettings));
        if (!ScaleDevice.IsConnected && localSettingsService.Simulate)
        {
            ScaleDevice = new ScaleSimulator();
            await ScaleDevice.ConnectAsync();
        }
    }
    public async Task InitializePadAsync()
    {
        await PadDevice.ConnectAsync(LoadSpi(localSettingsService.PadDeviceSettings));
    }

    public async Task InitializeCameraAsync()
    {
        var allMediaFrameSourceGroups = await CameraDevice.AllMediaFrameSourceGroups();
        if (allMediaFrameSourceGroups.Count > 0)
        {
            CameraDevice.SelectedMediaFrameSourceGroup = allMediaFrameSourceGroups[CameraDevice.CameraNumber];
            await CameraDevice.ConnectAsync();
        }
    }

    public async Task InitializeRobotAsync()
    {
        await RobotDevice.ConnectAsync(localSettingsService.RobotIpAddress);
        await RobotDevice.InitializeAsync();
        if (!RobotDevice.IsConnected && localSettingsService.Simulate)
        {

            RobotDevice = new RobotSimulator();
            await RobotDevice.ConnectAsync("simulate");
            await RobotDevice.InitializeAsync();
        }
    }
    public async Task InitializeAsync()
    {
        await InitializePortsAsync();
        await InitializeTemperatureAndHumidityAsync();
        await InitializeScaleAsync();
        await InitializePadAsync();
        await InitializeCameraAsync();
        await InitializeRobotAsync();
    }
    public void Dispose()
    {
        PadDevice.Dispose();
        CameraDevice.Dispose();
        HumidityDevice.Dispose();
        RobotDevice.Dispose();
        ScaleDevice.Dispose();
        TemperatureDevice.Dispose();
        GC.SuppressFinalize(this);
    }

    private SerialPortInfo LoadSpi(SerialPortInfo? setting)
    {
        if (setting is not null)
        {
            try
            {
                var realPort = AvailablePorts.First(spi => spi.PortName == setting.PortName);
                setting.Id = realPort?.Id;
                if (realPort is null)
                {
                    setting = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                setting = null;
            }
        }

        return setting ?? new SerialPortInfo();
    }
}
