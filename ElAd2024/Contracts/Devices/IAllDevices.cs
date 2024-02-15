using ElAd2024.Models;
using System.Collections.ObjectModel;

namespace ElAd2024.Contracts.Devices;
public interface IAllDevices : IDisposable
{
    ObservableCollection<SerialPortInfo> AvailablePorts { get; }
    ObservableCollection<string> AvailablePortsNames { get; }

    IPadDevice PadDevice { get; set; }
    ICameraDevice CameraDevice { get; set; }
    IHumidityDevice HumidityDevice { get; set; }
    IRobotDevice RobotDevice { get; set; }
    IScaleDevice ScaleDevice { get; set; }
    ITemperatureDevice TemperatureDevice  { get; set; }

    Task InitializeAsync();
    Task InitializePortsAsync();
    Task InitializeTemperatureAndHumidityAsync();
    Task InitializeScaleAsync();
    Task InitializePadAsync();
    Task InitializeCameraAsync();
    Task InitializeRobotAsync();

}
