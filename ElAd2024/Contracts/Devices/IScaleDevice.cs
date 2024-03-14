namespace ElAd2024.Contracts.Devices;

public interface IScaleDevice : IDevice
{
    bool IsStable { get; }
    int? Weight { get; }
    Task Tare();
    Task Zero();
}