using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Simulator;

public partial class ScaleSimulator : BaseSimulator, IScaleDevice
{
    private readonly Random random = new();
    [ObservableProperty] private bool isStable = true;
    [ObservableProperty] private int? weight;

    public async Task GetWeight()
    {
        Weight = random.Next(5000, 5200);
        await Task.CompletedTask;
    }

    public async Task Tare() => await Task.CompletedTask;

    public async Task Zero() => await Task.CompletedTask;
}