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

    private List<(int Number, int Value)> previousParameters = [];
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
        //await SendDataAsync("REL SBY");
        await ConsoleEcho(false);
        await GreenDebug(false);
        await SendDataAsync("R");
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

        foreach (var parameter in parametersList)
        {
            if (!previousParameters.Any(p => p.Number == parameter.Number && p.Value == parameter.Value))
            {
                await SendDataAsync($"SET {parameter.Number} {parameter.Value}");
            }
        }
        previousParameters.Clear();
        parametersList.ForEach(previousParameters.Add);

        // parametersList.ForEach(async parameter => await SendDataAsync($"SET {parameter.Number} {parameter.Value}"));
        await Task.CompletedTask;
    }

    public async Task StartCycle(bool isPlusPolarity, bool force = false)
    {


        InitializeChartDataCollection();
        if (force)
        {
            Commands.Clear();
        }

        await SendDataAsync(isPlusPolarity switch
        {
            true => "+",
            false => "-"
            //true => "PUL ST+",
            //false => "PUL ST-"
        });
    }

    public async Task StopCycle(bool force = true)
    {
        if (force)
        {
            Commands.Clear();
        }
        //await SendDataAsync("PUS DRP");
        await SendDataAsync("D");
    }

    public async Task ReleaseFabric(int wait = 1000, bool force = true)
    {
        await StopCycle(force);
        await Task.Delay(100);
        await Task.Delay((int)wait);
        //await SendDataAsync("REL SBY");
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

    protected async override Task SendDataAsync(string data)
    {
        Commands.Enqueue(data);
        if (Commands.Count == 1)
        {
            Debug.WriteLine($"Sending data: {Commands.Peek()}");
            await base.SendDataAsync(Commands.Peek());
        }
    }

    public async override Task Stop()
    {
        Commands.Clear();
        //await SendDataAsync("REL SBY");
        await SendDataAsync("R");
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
        else
        {
            Debug.WriteLine($"Received data line: {dataLine}");
            var sendNext = false;
            Debug.WriteLine($"dataLine: [{dataLine}], Commands.Peek(): [{(Commands.Count > 0 ? Commands.Peek() : "Commands EMPTY")}]");

            if (
                dataLine.StartsWith("OK") && Commands.Count > 0 && dataLine[3..] == Commands.Peek()
                || (dataLine == "OK PUL ST+" && Commands.Count > 0 && Commands.Peek() == "+")
                || (dataLine == "OK PUL ST-" && Commands.Count > 0 && Commands.Peek() == "-")
                || (dataLine == "OK PUS DRP" && Commands.Count > 0 && Commands.Peek() == "D")
                || (dataLine == "OK REL SBY" && Commands.Count> 0 && Commands.Peek() == "R")
                || (dataLine == "R" && Commands.Count > 0 && Commands.Peek() == "R")
              )
            {
                Debug.WriteLine($"OK: {Commands.Peek()}");
                Commands.Dequeue();
                sendNext = Commands.Count > 0;
            }
            else
            {
                Debug.WriteLine($"ERR: {(Commands.Count > 0 ? Commands.Peek() : "??")}");
                sendNext = Commands.Count > 0 && (dataLine.StartsWith("OK") || dataLine.StartsWith("ERR"));

            }
            if (sendNext)
            {
                Debug.WriteLine($"Sending next command ({(Commands.Count > 0 ? Commands.Peek() : "??")}), because: {dataLine}");
                await Task.Delay(10);
                await base.SendDataAsync(Commands.Peek());
            }
        }
    }
}