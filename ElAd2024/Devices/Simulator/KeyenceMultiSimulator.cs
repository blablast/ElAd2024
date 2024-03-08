using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Simulator;
internal class KeyenceMultiSimulator : BaseSimulator, IElectricFieldDevice, ITemperatureDevice, IHumidityDevice
{
    public float ElectricField => -5;

    public float Humidity => 0.5f;

    public float Temperature => 20;
}
