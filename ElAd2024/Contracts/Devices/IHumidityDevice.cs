using System.ComponentModel;

namespace ElAd2024.Contracts.Devices;

public interface IHumidityDevice : IDevice
{
    float Humidity { get; }
}