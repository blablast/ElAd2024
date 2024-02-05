
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElAd2024.Models;

public partial class TestParameters : ObservableRecipient
{
    [ObservableProperty] private uint maxHighVoltage = 10000;
    [ObservableProperty] private uint highVoltagePhase1 = 5000;
    [ObservableProperty] private uint highVoltagePhase3 = 5000;
    [ObservableProperty] private uint maxDuration = 10000;
    [ObservableProperty] private uint durationPhase1 = 5000;
    [ObservableProperty] private uint durationPhase2 = 1000;
    [ObservableProperty] private uint durationPhase3 = 5000;
    [ObservableProperty] private uint durationPhase4 = 10000;
    [ObservableProperty] private uint loadForce = 10;
    [ObservableProperty] private bool autoRegulationHV = false;

    [ObservableProperty] private bool isStartPolarizationPlus = true;
    [ObservableProperty] private bool isSwitchWorkMode = true;
    [ObservableProperty] private int changePolarizationStep = 1;

    [ObservableProperty] private uint count = 20;
}
