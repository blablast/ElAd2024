using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Core.Models;
using ElAd2024.Models;
using Microsoft.UI.Dispatching;
using Newtonsoft.Json.Linq;

namespace ElAd2024.ViewModels;

public partial class PadDataViewModel : BaseSerialDataViewModel
{
    public enum PulType
    {
        Switch,
        Plus,
        Minus,
       }
    private readonly uint dataCollectionSize;
    
    public List<Voltage> Voltages { get; set; } = [];
    public ObservableCollection<HVPlot> ChartDataCollection { get; set; } = [];

    [ObservableProperty] private int? highVoltage;
    [ObservableProperty] private byte phaseNumber;
    [ObservableProperty] private long elapsedTime;

    [ObservableProperty] private int axisMinVoltage = -7000;
    [ObservableProperty] private int axisMaxVoltage = +7000;

    public bool Paused { get; set; } = false;

    private readonly DispatcherQueue dispatcherQueue;

    public PadDataViewModel(SerialPortInfo serialPortInfo, uint dataCollectionSize = 120) : base(serialPortInfo)
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        this.dataCollectionSize = dataCollectionSize;
        InitializeChartDataCollection();
    }

    private void InitializeChartDataCollection()
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            ChartDataCollection.Clear();
            for (var i = 0; i < dataCollectionSize; i++)
            {
                ChartDataCollection.Add(new HVPlot { ElapsedTime = (i - dataCollectionSize + 1), PhaseNumber = 0 });
            }
        });
        Voltages = [];
        Voltages.Add(new Voltage { PhaseNumber = 1, ElapsedTime = 0, HighVoltage = 0 });
    }

    public async Task Setup(List<(int Number, int Value)> parameters)
    {
        parameters.ForEach(async (parameter) => await SendDataAsync($"SET {parameter.Number} {parameter.Value}"));
        await Task.CompletedTask;
    }

    public async Task StartCycle(bool isPlusPolarity)
                => await StartCycle(isPlusPolarity ?  PulType.Plus : PulType.Minus);

    public async Task StartCyclePadPolarity()
        => await StartCycle(PulType.Switch);


    private async Task StartCycle(PulType type = PulType.Switch)
    {
        InitializeChartDataCollection();
        Paused = false;
        await SendDataAsync(type switch
        {
            PulType.Switch => "PUL ST*",
            PulType.Plus =>   "PUL ST+",
            PulType.Minus =>  "PUL ST-",
            _ => "PUL ST*"
        });
    }
    public async Task StopCycle()
        => await SendDataAsync("PUS DRP");
    protected async override Task StopDevice()
    {
        await Task.Delay(100);
        await StopCycle();
    }

    protected override void ProcessDataLine(string dataLine)
    {
        if (dataLine.StartsWith('A'))
        {
            Debug.WriteLine(dataLine);
            var parts = dataLine[2..].Split(',');
            if (parts.Length == 3
                && byte.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var phaseNumber)
                && uint.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var elapsedTime)
                && int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var highVoltage)
                )
            {
                if (!Paused)
                {
                    Voltages.Add(new Voltage { PhaseNumber = phaseNumber, ElapsedTime = elapsedTime, HighVoltage = highVoltage });
                }

                dispatcherQueue.TryEnqueue(() =>
                {
                    PhaseNumber = phaseNumber;
                    ElapsedTime = elapsedTime;
                    HighVoltage = highVoltage;
                });

                if (ChartDataCollection.Count >= dataCollectionSize) {
                    dispatcherQueue.TryEnqueue(() => { ChartDataCollection.RemoveAt(0); });
                }

                var point = new HVPlot
                {
                    ElapsedTime = elapsedTime,
                    PhaseNumber = phaseNumber,
                    HighVoltage = highVoltage,
                    HighVoltagePhase4 = highVoltage
                };

                if (phaseNumber < 4) { point.HighVoltagePhase3 = highVoltage; }
                if (phaseNumber < 3) { point.HighVoltagePhase2 = highVoltage; }
                if (phaseNumber < 2) { point.HighVoltagePhase1 = highVoltage; }
                
                dispatcherQueue.TryEnqueue(() => {
                    AxisMinVoltage = Math.Min(AxisMinVoltage, (int)(-1.1 * highVoltage / 100) * 100);
                    AxisMaxVoltage = Math.Max(AxisMaxVoltage, (int)(1.1 * highVoltage / 100) * 100);
                    ChartDataCollection.Add(point);
                });
            }
        }
        else if (dataLine.StartsWith("OK GET"))
        {
            var parts = dataLine.Split(' ');
            if (parts.Length >= 4
                && int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var parameter)
                && int.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            {
                if (parameter == 1 || parameter == 2)
                {
                    dispatcherQueue.TryEnqueue(() =>
                    {
                        AxisMinVoltage = Math.Min(AxisMinVoltage, (int)(-1.1 * value / 100) * 100);
                        AxisMaxVoltage = Math.Max(AxisMaxVoltage, (int)(1.1 * value / 100) * 100);
                    });
                }
            }
        }
        else
        {
            ReceivedData += dataLine + '\n';
        }
    }
}