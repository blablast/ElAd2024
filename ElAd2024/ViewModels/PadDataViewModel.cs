using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Core.Models;
using ElAd2024.Models;

namespace ElAd2024.ViewModels;

public partial class PadDataViewModel : BaseSerialDataViewModel
{
    private readonly uint dataCollectionSize;
    public ObservableCollection<HVPlot> ChartDataCollection { get; set; }

    [ObservableProperty] private int? highVoltage;
    [ObservableProperty] private byte phaseNumber;
    [ObservableProperty] private long elapsedTime;

    [ObservableProperty] private int axisMinVoltage = -7000;
    [ObservableProperty] private int axisMaxVoltage = +7000;

    public PadDataViewModel(SerialPortInfo serialPortInfo, uint dataCollectionSize = 120) : base(serialPortInfo)
    {
        this.dataCollectionSize = dataCollectionSize;
        ChartDataCollection = [];
        InitializeChartDataCollection();
    }

    private void InitializeChartDataCollection()
    {
        ChartDataCollection.Clear();
        for (var i = 0; i < dataCollectionSize; i++)
        {
            ChartDataCollection.Add(new HVPlot { ElapsedTime = (i - dataCollectionSize + 1), PhaseNumber = 0 });
        }
    }
    public async Task StartCycle()
    {
        InitializeChartDataCollection();
        await SendDataAsync("GET 1");
        await SendDataAsync("GET 2");
        await SendDataAsync("PUL ST*");
    }
    public async Task StopCycle()
        => await SendDataAsync("PUS DRP");
    protected async override Task OnConnected()
        => await Task.Delay(100);

    protected override void ProcessDataLine(string dataLine)
    {
        if (dataLine.StartsWith('A'))
        {
            var parts = dataLine[2..].Split(',');
            if (parts.Length == 3
                && byte.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var phaseNumber)
                && uint.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var elapsedTime)
                && int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var highVoltage)
                )
            {
                PhaseNumber = phaseNumber;
                ElapsedTime = elapsedTime;
                HighVoltage = highVoltage;

                if (ChartDataCollection.Count >= dataCollectionSize) { ChartDataCollection.RemoveAt(0); }

                var point = new HVPlot
                {
                    ElapsedTime = elapsedTime,
                    PhaseNumber = phaseNumber,
                    HighVoltagePhase4 = highVoltage
                };

                if (phaseNumber < 4) { point.HighVoltagePhase3 = highVoltage; }
                if (phaseNumber < 3) { point.HighVoltagePhase2 = highVoltage; }
                if (phaseNumber < 2) { point.HighVoltagePhase1 = highVoltage; }

                ChartDataCollection.Add(point);
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
                    AxisMinVoltage = Math.Min(AxisMinVoltage, (int)(-1.3 * value));
                    AxisMaxVoltage = Math.Max(AxisMaxVoltage, (int)(1.3 * value));
                }
            }
        }
        else
        {
            ReceivedData += dataLine + '\n';
        }
    }
}