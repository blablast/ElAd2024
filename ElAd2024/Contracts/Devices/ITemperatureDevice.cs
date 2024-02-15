namespace ElAd2024.Contracts.Devices;

public interface ITemperatureDevice : IDevice
{
    float Temperature { get; }
}