using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using Microsoft.UI.Dispatching;

namespace ElAd2024.Devices.Serial;
public partial class KeyenceMultiDevice : BaseSerialDevice, IElectricFieldDevice //, ITemperatureDevice, IHumidityDevice
{
    [ObservableProperty] private int value;
    //[ObservableProperty] private float temperature;
    //[ObservableProperty] private float humidity;

    //private readonly DispatcherQueue dispatcherQueue;

    public Queue<string> Commands { get; set; } = [];

    public KeyenceMultiDevice()
    {
        //dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async new Task GetData(bool forceClear)
    {
        if (forceClear)
        {
            Commands.Clear();
        }

        await SendDataAsync("MS");
        //await SendDataAsync("SR,00,045");
        //await SendDataAsync("SR,00,046");
    }

    protected async override Task SendDataAsync(string data)
    {
        Commands.Enqueue(data);
        if (Commands.Count == 1)
        {
            await base.SendDataAsync(Commands.Peek());
        }
    }

    protected async override void ProcessDataLine(string dataLine)
    {
        if (dataLine.StartsWith("MS"))
        {
            var parts = dataLine.Split(',');
            if (parts.Length == 3 && double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var electricFieldValue))
            {
                try
                {
                    Value = (int)((float)electricFieldValue * 1000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                //dispatcherQueue.TryEnqueue(() => { Value = (int)((float)electricFieldValue*1000); });
            }
        }
        //else if (dataLine.StartsWith("SR,00,045"))
        //{
        //    var parts = dataLine.Split(',');
        //    if (parts.Length == 4 && double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var temperatureValue))
        //    {
        //        dispatcherQueue.TryEnqueue(() => { Temperature = (float)temperatureValue; });
        //    }
        //}
        //else if (dataLine.StartsWith("SR,00,046"))
        //{
        //    var parts = dataLine.Split(',');
        //    if (parts.Length == 4 && double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var humidityValue))
        //    {
        //        dispatcherQueue.TryEnqueue(() => { Humidity = (float)humidityValue; });
        //    }
        //}

        if (Commands.Count > 0)
        {
            await Task.Delay(20);
            await base.SendDataAsync(Commands.Peek());
        }
    }
}
