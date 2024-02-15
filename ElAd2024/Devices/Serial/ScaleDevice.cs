using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Serial;

public partial class ScaleDevice : BaseSerialDevice, IScaleDevice
{
    private bool isReading;
    [ObservableProperty] private bool isStable;
    [ObservableProperty] private int? weight;

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
            if (!dataLine.StartsWith("ST") || !dataLine.StartsWith("UT")) // Tare
            {
                Debug.WriteLine($"ScaleDataViewModel->ProcessDataLine: No match for '{dataLine}'");
            }
        }

        isReading = false;
    }
}