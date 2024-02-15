using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Simulator;
public partial class HumidityAndTemperatureSimulator : BaseSimulator, ITemperatureDevice, IHumidityDevice
{
    [ObservableProperty] private float temperature = 22.22f;
    [ObservableProperty] private float humidity = 33.33f;
}