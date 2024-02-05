using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Models;

namespace ElAd2024.ViewModels;
public partial class EnvDataViewModel(SerialPortInfo serialPortInfo) : BaseSerialDataViewModel(serialPortInfo)
{
    [ObservableProperty] private float temperature;
    [ObservableProperty] private float humidity;

    protected async override Task OnConnected()
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
            if (parts.Length == 2)
            {
                UpdateEnvironmentalData(parts);
            }
        }
        else
        {
            ReceivedData += dataLine + '\n';
        }
    }

    // Method to update temperature and humidity
    private void UpdateEnvironmentalData(string[] parts)
    {
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