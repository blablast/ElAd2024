using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Serial;
public partial class HumidityAndTemperatureDevice : BaseSerialDevice, ITemperatureDevice, IHumidityDevice
{
    [ObservableProperty] private float temperature;
    [ObservableProperty] private float humidity;

    public async override Task Stop()
    {
        await Task.Delay(1000);
        await SendDataAsync("SEND AUTO ON");
    }

    protected async override Task OnDisconnecting()
        => await SendDataAsync("SEND AUTO OFF");

    protected override void ProcessDataLine(string dataLine)
    {
        if (dataLine.StartsWith('A'))
        {
            var parts = dataLine[2..].Split(',');
            if (parts.Length == 3)
            {
                UpdateEnvironmentalData(parts);
            }
        }
    }

    // Method to update temperature and humidity
    private void UpdateEnvironmentalData(IReadOnlyList<string> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);

        if (float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var temp))
        {
            Temperature = temp;
        }
        if (float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var hum))
        {
            Humidity = hum;
        }
    }
}