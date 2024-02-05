using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Core.Models;
using ElAd2024.Models;

namespace ElAd2024.ViewModels;

public partial class ScaleDataViewModel(SerialPortInfo serialPortInfo) : BaseSerialDataViewModel(serialPortInfo)
{
    [ObservableProperty] private bool isStable;
    [ObservableProperty] private int? weight;
    private bool isReading;

    public async Task GetWeight()
    {
        if (!isReading)
        {
            isReading = true;
            await SendDataAsync("Sx3");
        }
    }
    public async Task Tare()
        => await SendDataAsync("ST");
    public async Task Zero()
        => await SendDataAsync("SZ");
    
    [GeneratedRegex("-?\\s*\\d+")]
    private static partial Regex ScaleWeightRegex();

    protected override void ProcessDataLine(string dataLine)
    {
        var match = ScaleWeightRegex().Match(dataLine);
        if (match.Success)
        {
            var numericPart = match.Value.Replace(" ", "");
            if (int.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var weight))
            {
                Weight = weight;
                IsStable = dataLine.StartsWith('S');
            }
        }
        else
        {
            IsStable = false;
            Weight = null;
            if (!dataLine.StartsWith("ST") || !dataLine.StartsWith("UT"))   // Tare
            {
                Debug.WriteLine($"ScaleDataViewModel->ProcessDataLine: No match for '{dataLine}'");
            }
        }
        isReading = false;
    }
}