using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using ElAd2024.Models.Database;
using Microsoft.UI.Dispatching;

namespace ElAd2024.Devices.Serial;

public partial class PadDevice : BaseSerialDevice, IPadDevice
{
    private readonly int dataCollectionSize;
    private readonly DispatcherQueue dispatcherQueue;

    private readonly List<(int Number, int Value)> previousParameters = [];
    private readonly Dictionary<string, string> shortCommands = new() { ["REL SBY"] = "R", ["PUL ST+"] = "+", ["PUL ST-"] = "-", ["PUS DRP"] = "D" };

    public Queue<string> Commands { get; set; } = [];


    [ObservableProperty] private int axisMaxVoltage = +12000;
    [ObservableProperty] private int axisMinVoltage = -12000;
    [ObservableProperty] private int elapsed;
    [ObservableProperty] private byte phase;
    [ObservableProperty] private int? value;
    private int index = 0;

    public PadDevice(int dataCollectionSize = 250)
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.dataCollectionSize = dataCollectionSize;
        InitializeChartDataCollection();
        DeviceService.Delay = 20;
    }



    protected async override Task OnConnected()
    {
        await ConsoleEcho(false);
        await GreenDebug(false);
        await SendRelSby();
    }

    private async Task GreenDebug(bool isOn = false) => await SendSet(12, $"{(isOn ? 1 : 0)}");
    private async Task ConsoleEcho(bool isOn = false) => await SendSet(13, $"{(isOn ? 0 : 1)}");

    public ObservableCollection<Voltage> Voltages { get; set; } = [];

    public async Task Setup(object parameters)
    {
        if (parameters is not List<(int Number, int Value)> parametersList)
        {
            throw new ArgumentException("Invalid parameters type.", nameof(parameters));
        }

        foreach (var parameter in parametersList)
        {
            if (!previousParameters.Any(p => p.Number == parameter.Number && p.Value == parameter.Value))
            {
                await SendSet(parameter.Number, parameter.Value.ToString());
                await Task.Delay(DeviceService.Delay);
            }
        }
        previousParameters.Clear();
        foreach (var parameter in parametersList)
        {
            previousParameters.Add(parameter);
        }

        // parametersList.ForEach(async parameter => await SendDataAsync($"SET {parameter.Number} {parameter.Value}"));
        await Task.CompletedTask;
    }

    public async Task StartCycle(bool isPlusPolarity, bool force = false)
    {
        InitializeChartDataCollection();
        if (force) { Commands.Clear(); }
        if (isPlusPolarity) { await SendStPlus(); } else { await SendStMinus(); }
    }

    public async Task StopCycle(bool force = true)
    {
        if (force) { Commands.Clear(); }
        await SendPusDrp();
    }

    public async Task ReleaseFabric(int wait = 1000, bool force = true)
    {
        await StopCycle(force);
        await Task.Delay(100);
        await Task.Delay((int)wait);
        await Stop();

        dispatcherQueue.TryEnqueue(() =>
        {
            var itemsToRemove = Voltages.Where(v => v.Phase == 0).ToList();
            foreach (var item in itemsToRemove)
            {
                Voltages.Remove(item);
            }
        });
        await Task.CompletedTask;
    }



    private void InitializeChartDataCollection() =>
        dispatcherQueue.TryEnqueue(() =>
        {
            Voltages.Clear();
            Voltages.Add(new Voltage { Elapsed = 0, Phase = 1 });
            index = 1;
            for (var i = index; i < dataCollectionSize; i++)
            {
                Voltages.Add(new Voltage { Elapsed = i, Phase = 0 });
            }
        });


    private async Task SendSet(int index, string value) => await SendDataAsync($"SET {index} {value}");
    private async Task SendStPlus() => await SendDataAsync("PUL ST+");
    private async Task SendStMinus() => await SendDataAsync("PUL ST-");
    private async Task SendPusDrp() => await SendDataAsync("PUS DRP");
    private async Task SendRelSby() => await SendDataAsync("REL SBY");



    private async Task Send()
    {
        if (Commands.Count > 0)
        {
            await base.SendDataAsync(shortCommands.ContainsKey(Commands.Peek()) ? shortCommands[Commands.Peek()] : Commands.Peek());
        }
    }

    protected async override Task SendDataAsync(string data)
    {
        Commands.Enqueue(data);
        if (Commands.Count == 1)
        {
            await Send();
        }
    }

    public async override Task Stop()
    {
        Commands.Clear();
        await SendRelSby();
    }

    protected async override void ProcessDataLine(string dataLine)
    {
        if (dataLine.StartsWith("A:"))
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
        else if (Commands.Count > 0)
        {
            if (dataLine.StartsWith("OK") && dataLine.Length > 4 && dataLine[3..] == Commands.Peek())
            {
                Commands.Dequeue();
                await Send();
            }
            else if (dataLine.StartsWith("OK") || dataLine.StartsWith("ERR"))
            {
                Debug.WriteLine($"dataLine.StartsWith(\"OK\") || dataLine.StartsWith(\"ERR\") : {dataLine}, {(Commands.Count>0 ? Commands.Peek() : "")}");
                await Send();
            }
        }
    }
}