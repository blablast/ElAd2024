using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Simulator;
internal class KeyenceMultiSimulator : BaseSimulator, IElectricFieldDevice //, ITemperatureDevice, IHumidityDevice
{
    public static float Humidity => 0.5f;

    public static float Temperature => 20;

    public int Value => -5;

    public async new Task GetData(bool forceClear = false) => await Task.CompletedTask;
}
