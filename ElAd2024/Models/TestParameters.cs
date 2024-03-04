
using CommunityToolkit.Mvvm.ComponentModel;

namespace ElAd2024.Models;

public partial class TestParameters : ObservableRecipient
{
    public static int MinHighVoltage => 1000;
    public static int MaxHighVoltage => 10000;
    public static int MinDuration => 200;
    public static int MaxDuration => 5000;

    [ObservableProperty] private int durationPhase1 = 2000;
    [ObservableProperty] private int durationPhase1Lo = 2000;
    [ObservableProperty] private int durationPhase1Hi = 3000;
    [ObservableProperty] private bool durationPhase1MaxToMin = true;

    [ObservableProperty] private int highVoltagePhase1 = 5000;
    [ObservableProperty] private int highVoltagePhase1Lo = 4000;
    [ObservableProperty] private int highVoltagePhase1Hi = 7000;
    [ObservableProperty] private bool highVoltagePhase1MaxToMin = true;

    [ObservableProperty] private int durationPhase2 = 500;
    [ObservableProperty] private int durationPhase2Lo = 500;
    [ObservableProperty] private int durationPhase2Hi = 500;
    [ObservableProperty] private bool durationPhase2MaxToMin = true;

    [ObservableProperty] private int durationPhase3 = 2000;
    [ObservableProperty] private int durationPhase3Lo = 2000;
    [ObservableProperty] private int durationPhase3Hi = 3000;
    [ObservableProperty] private bool durationPhase3MaxToMin = true;

    [ObservableProperty] private int highVoltagePhase3 = 5000;
    [ObservableProperty] private int highVoltagePhase3Lo = 6000;
    [ObservableProperty] private int highVoltagePhase3Hi = 8000;
    [ObservableProperty] private bool highVoltagePhase3MaxToMin = false;

    [ObservableProperty] private int loadForce = 30;
    [ObservableProperty] private bool autoRegulationHV = true;

    [ObservableProperty] private bool isStartPlusPolarity = true;
    [ObservableProperty] private int  changePolarityStep = 1;

    [ObservableProperty] private int total = 10;
    [ObservableProperty] private int counter = 0;
}
