using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using ElAd2024.Models.Database;
using Microsoft.UI.Dispatching;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElAd2024.Devices.Serial;

public partial class PadDevice : BaseSerialDevice, IPadDevice
{
    private readonly int dataCollectionSize;
    private readonly DispatcherQueue dispatcherQueue;
    private bool isRunning = false;

    [ObservableProperty] private int axisMaxVoltage = +3000;
    [ObservableProperty] private int axisMinVoltage = -3000;
    [ObservableProperty] private int elapsed;
    [ObservableProperty] private byte phase;
    [ObservableProperty] private int? value;
    private int index = 0;

    public PadDevice(int dataCollectionSize = 120)
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.dataCollectionSize = dataCollectionSize;
        InitializeChartDataCollection();
    }

    protected async override Task OnConnected()
    {
        await SendDataAsync("PUS DRP");
        await GreenDebug(false);
        await ConsoleEcho(false);
    }

    private async Task GreenDebug(bool isOn = false)
        => await SendDataAsync($"SET 12 {(isOn ? 1 : 0)}");
    private async Task ConsoleEcho(bool isOn = false)
        => await SendDataAsync($"SET 13 {(isOn ? 0 : 1)}");


    public ObservableCollection<Voltage> Voltages { get; set; } = [];

    public async Task Setup(object parameters)
    {
        if (parameters is not List<(int Number, int Value)> parametersList)
        {
            throw new ArgumentException("Invalid parameters type.", nameof(parameters));
        }

        parametersList.ForEach(async parameter => await SendDataAsync($"SET {parameter.Number} {parameter.Value}"));
        await Task.CompletedTask;
    }

    public async Task StartCycle(bool isPlusPolarity)
    {
        InitializeChartDataCollection();
        await SendDataAsync(isPlusPolarity switch
        {
            true => "PUL ST+",
            false => "PUL ST-"
        });
        isRunning = true;
    }

    public async Task StopCycle(bool trimData)
    {
        isRunning = false;
        if (trimData)
        {
            dispatcherQueue.TryEnqueue(() =>
            {
                var itemsToRemove = Voltages.Where(v => v.Phase == 0).ToList();
                foreach (var item in itemsToRemove)
                {
                    Voltages.Remove(item);
                }
            });
        }
        await SendDataAsync("PUS DRP");
    }

    private void InitializeChartDataCollection() =>
        dispatcherQueue.TryEnqueue(() =>
        {
            Voltages.Clear();
            for (var i = 0; i < dataCollectionSize; i++)
            {
                Voltages.Add(new Voltage { Elapsed = i - dataCollectionSize + 1, Phase = 0 });
            }
            index = 0;
        });

    private async Task SendAndWait(string data)
    {
        await base.SendDataAsync(data);
        await Task.Delay(5);
    }

    protected async override Task SendDataAsync(string data)
    {
        Debug.WriteLine($"SendDataAsync: {data}");

        var version = 2;

        switch (version)
        {
            case 1:
                await SendAndWait(data);
                break;

            case 2:
                var shortData = new StringBuilder();
                var maxLength = 2;
                foreach (var character in data)
                {
                    shortData.Append(character);
                    if (shortData.Length == maxLength)
                    {
                        await SendAndWait(shortData.ToString());
                        shortData.Clear();
                    }
                }

                if (shortData.Length > 0)
                {
                    await SendAndWait(shortData.ToString());
                }

                break;
        }


        await base.SendDataAsync(data.ToString());
        await Task.Delay(5);


    }

    protected async override Task StopDevice()
    {
        await Task.Delay(100);
        await StopCycle(false);
    }

    protected override void ProcessDataLine(string dataLine)
    {
        Debug.WriteLine($"ProcessDataLine: {dataLine}");
        if (isRunning && dataLine.StartsWith('A'))
        {
            var parts = dataLine[2..].Split(',');
            if (parts.Length == 3
                && byte.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var phaseNumber)
                && int.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var elapsedTime)
                && int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var highVoltage)
               )
            {
                dispatcherQueue.TryEnqueue((DispatcherQueueHandler)(() =>
                {
                    Phase = phaseNumber;
                    Elapsed = elapsedTime;
                    Value = highVoltage;
                    if (index < Voltages.Count)
                    {
                        Voltages.RemoveAt(index);
                        Voltages.Insert(index++, new Voltage { Phase = phaseNumber, Elapsed = elapsedTime, Value = highVoltage });
                    }
                    else
                    {
                        Voltages.Add(new Voltage { Phase = phaseNumber, Elapsed = elapsedTime, Value = highVoltage });
                    }

                    static double round(double value) => (value >= 0) ? Math.Ceiling(value) : Math.Floor(value);

                    AxisMaxVoltage = Math.Max(AxisMaxVoltage, (int)round(1.1 * highVoltage / 1000.0) * 1000);
                    AxisMinVoltage = Math.Min(AxisMinVoltage, (int)round(1.1 * highVoltage / 1000.0) * 1000);
                }));
            }
        }
    }
}