using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using ElAd2024.Contracts.Services;
using ElAd2024.Devices;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.Models.Database;
using ElAd2024.ViewModels;
using Microsoft.UI.Dispatching;

namespace ElAd2024.Services;

public partial class ProceedTestService : ObservableRecipient, IProceedTestService
{

    #region IDisposable

    public void Dispose()
    {

        AllDevices.PadDevice.PropertyChanged -= PadDevice_PropertyChanged;
        AlgorithmSteps?.Stop();
        robotTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region Fields
    private readonly ILocalSettingsService localSettingsService;
    private readonly IDatabaseService databaseService;
    private readonly DispatcherQueue dispatcherQueue;

    private Timer? robotTimer;
    private Timer? videoTimer;
    private bool isRobotTimerOn;
    private readonly int robotTimerPeriod = 100;

    #endregion Fields

    #region Properties

    // Collection of test steps
    public ObservableCollectionNotifyPropertyChange<AlgorithmStepViewModel> AlgorithmSteps { get; set; } = [];

    [ObservableProperty] private int currentStep = -1;

    [ObservableProperty] private IProceedTestService.ErrorType error;
    [ObservableProperty] private bool isRunning;

    [ObservableProperty] private IAllDevices allDevices;
    [ObservableProperty] private TestParameters? parameters;
    [ObservableProperty] private Test currentTest = new();

    #endregion Properties

    #region Constructor

    public ProceedTestService(IAllDevices allDevices, ILocalSettingsService localSettingsService, IDatabaseService databaseService)
    {
        this.databaseService = databaseService;
        this.localSettingsService = localSettingsService;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        AllDevices = allDevices;

        // Initialize timers
    }


    #endregion Constructor

    #region Public Methods
    public async Task StartTest()
    {
        ArgumentNullException.ThrowIfNull(Parameters, nameof(Parameters));
        if (AllDevices.ScaleDevice.Weight == 0)
        {
            Error = IProceedTestService.ErrorType.WeightIsZero;
            return;
        }
        IsRunning = true;
        AllDevices.PadDevice.PropertyChanged += PadDevice_PropertyChanged;

        robotTimer = new Timer(UpdateRobotPosition, null, Timeout.Infinite, 0);
        videoTimer = new Timer(VideoElapsed, null, Timeout.Infinite, 0);

        CurrentStep = 0;
        await CurrentStepChanged();
    }
    public async Task InitializeStepsAsync(int algorithmId = 1)
    {
        AlgorithmSteps.Stop();
        AlgorithmSteps.Clear();
        var dbAs = databaseService.AlgorithmSteps?.Where(a => a.AlgorithmId == algorithmId).OrderBy(a => a.Order).ToList() ?? [];
        foreach (var algorithmStep in dbAs)
        {
            AlgorithmSteps.Add(new AlgorithmStepViewModel(algorithmStep));
        }
        AlgorithmSteps.Start(OnAlgorithmStepChanged);
        dispatcherQueue.TryEnqueue(() =>
        {
            AlgorithmSteps.ToList().ForEach(step => step.Reset());
            AlgorithmSteps.ToList().ForEach(step => step.Opacity = 0.5);
        });
        await Task.CompletedTask;
    }

    #endregion Public Methods

    #region Private Methods
    private Func<string?, Task> GetMethodAsFunc(string methodName)
    {
        var methodInfo =
            typeof(ProceedTestService).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
            throw new ArgumentException($"Method '{methodName}' not found.");
        return (Func<string?, Task>)Delegate.CreateDelegate(typeof(Func<string?, Task>), this, methodInfo);
    }

    private async Task CurrentStepChanged()
    {
        if (CurrentStep < AlgorithmSteps?.Count)
        {
            var method = GetMethodAsFunc(AlgorithmSteps[CurrentStep].AlgorithmStep!.Step.AsyncActionName);
            CurrentTest.TestSteps.Add(
                       new TestStep
                       {
                           Step = AlgorithmSteps[CurrentStep].AlgorithmStep!.Step,
                           ActionParameter = AlgorithmSteps[CurrentStep].AlgorithmStep!.ActionParameter,
                           FrontName = AlgorithmSteps[CurrentStep].AlgorithmStep!.FrontName
                       });
            await method(AlgorithmSteps[CurrentStep].ActionParameter);
        }
    }
    private void OnAlgorithmStepChanged(object? sender, PropertyChangedEventArgs e)
        => dispatcherQueue?.TryEnqueue(() => AlgorithmSteps?.ForceRefresh(sender));

    private async void PadDevice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsRunning && e.PropertyName == nameof(AllDevices.PadDevice.Phase) && AllDevices.PadDevice.Phase == 4)
        {
            await DeadStep($"Done!\n{AllDevices.PadDevice.Voltages.LastOrDefault(v => v.Phase == AllDevices.PadDevice.Phase - 1)?.Value}[V]");
        }
    }

    #endregion Private Methods

    #region Process Cycle

    private async Task AliveStep(string? message = null)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            AlgorithmSteps[CurrentStep].Opacity = 1;
            if (message?.Length > 0)
            {
                AlgorithmSteps[CurrentStep].BackName = message;
            }
            AlgorithmSteps[CurrentStep].IsFrozen = false;
        });
        await Task.Delay(1);
    }

    private async void VideoElapsed(object? state)
    {
        await AllDevices.MediaDevice.StopRecording();
        videoTimer?.Change(Timeout.Infinite, 0);
        await DeadStep();
    }

    private async Task GetVideo(string? obj)
    {
        if (!int.TryParse(obj, out var duration))
        {
            throw new ArgumentException("Invalid parameters in function GetVideo()");
        }
        await AliveStep();
        var fileName = await AllDevices.MediaDevice.StartRecording($"{CurrentTest.Id:D5}");
        CurrentTest.Videos.Add(new Video { FileName = fileName, Description = obj ?? string.Empty });
        videoTimer?.Change(duration * 1000, 0);
    }

    private async Task TakeFabric(string? _)
    {
        await AliveStep();
        await AllDevices.PadDevice.StartCycle(CurrentTest.IsPlusPolarity);
    }

    private async void UpdateRobotPosition(object? _)
    {
        if (!isRobotTimerOn)
        {
            isRobotTimerOn = true;
            if (await AllDevices.RobotDevice.GetFlagRegisterAsync(localSettingsService.RobotInPositionRegister))
            {
                robotTimer?.Change(Timeout.Infinite, 0);
                await DeadStep();
            }
            else
            {
                var xyzwprPosition = AllDevices.RobotDevice.CurrentPosition;
                if (xyzwprPosition.IsValid)
                {
                    await AliveStep($"Position:\nx: {xyzwprPosition.X}\ny: {xyzwprPosition.Y}\nz: {xyzwprPosition.Z}");
                }
            }
            isRobotTimerOn = false;
        }
    }

    private async Task DeadStep(string? message = null)
    {
        dispatcherQueue.TryEnqueue(async () =>
        {
            AlgorithmSteps[CurrentStep].FlipSlow();
            AlgorithmSteps[CurrentStep].BackName = message ?? $"{AlgorithmSteps[CurrentStep].BackName.Replace("...", ".")}\nDone!";
            if (CurrentStep < AlgorithmSteps.Count - 1)
            {
                CurrentStep++;
                await CurrentStepChanged();
            }
        });
        await Task.CompletedTask;
    }

    private async Task Finish(string? _)
    {
        await AliveStep();
        CurrentTest.Voltages = new List<Voltage>(AllDevices.PadDevice.Voltages);
        IsRunning = false;
        await DeadStep();
        robotTimer?.Dispose();
        videoTimer?.Dispose();
        AllDevices.PadDevice.PropertyChanged -= PadDevice_PropertyChanged;
    }

    private async Task GetHumidity(string? _)
    {
        await AliveStep();
        CurrentTest.Humidities.Add(new Humidity() { Value = AllDevices.HumidityDevice.Humidity });
        await DeadStep($"{AllDevices.HumidityDevice.Humidity,2}%");
    }

    private async Task GetPhoto(string? obj)
    {
        await AliveStep();

        var (fileName, fullPath) = await AllDevices.MediaDevice.CapturePhoto($"{CurrentTest.Id:D5}_{obj?.Replace(" ", "_")}");
        CurrentTest.Photos.Add(
            new Photo
            {
                FileName = fileName,
                FullPath = fullPath,
                Description = obj ?? string.Empty
            });
        dispatcherQueue?.TryEnqueue(() =>
        {
            AlgorithmSteps[CurrentStep].ImageSource = fullPath;
        });
        await DeadStep();
    }

    private async Task GetTemperature(string? _)
    {
        await AliveStep();
        CurrentTest.Temperatures.Add(new Temperature() { Value = AllDevices.TemperatureDevice.Temperature });
        await DeadStep($"{AllDevices.TemperatureDevice.Temperature,2}°C");
    }

    private async Task GetWeight(string? obj)
    {
        await AliveStep();
        var weight = AllDevices.ScaleDevice.Weight;
        if (weight is not null)
        {
            CurrentTest.Weights.Add(new Weight { Value = weight.Value, Description = obj ?? string.Empty });
        }
        await DeadStep($"{weight}g");
    }

    private async Task Wait(string? durationString)
    {
        if (!int.TryParse(durationString, out var duration) || duration <= 0)
        {
            throw new ArgumentException("Invalid parameters in function Wait()");
        }

        await AliveStep($"{duration / 1000:F1}s.");
        await Task.Delay(duration);
        await DeadStep();
    }

    private async Task ReleaseFabric(string? _)
    {
        await AliveStep();
        await AllDevices.PadDevice.ReleaseFabric(CurrentTest.IsPlusPolarity);
        await DeadStep();
    }

    private async Task RobotMove(string? obj)
    {
        if (!int.TryParse(obj, out var position))
        {
            throw new ArgumentException("Invalid parameters in function RobotMoveTo()");
        }
        await AliveStep($"Moving to\n{position}...");

        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotInPositionRegister, false);
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotGotoPositionRegister, position);
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotRunRegister, true);
        robotTimer?.Change(robotTimerPeriod, robotTimerPeriod);
    }

    private async Task Start(object? _)
    {
        AlgorithmSteps.ToList().ForEach(step => step.Reset());
        await AliveStep();
        await AllDevices.PadDevice.Setup(new List<(int Number, int Value)>
        {
            (1, Parameters!.HighVoltagePhase1),
            (2, Parameters.HighVoltagePhase3),
            (4, (int)(Parameters.DurationPhase1 / 100)),
            (5, (int)(Parameters.DurationPhase2 / 100)),
            (6, (int)(Parameters.DurationPhase3 / 100)),
            (8, Parameters.AutoRegulationHV ? 1 : 0),
            (15, Parameters.AutoRegulationMaxCorrectionUp),
            (16, Parameters.AutoRegulationMaxCorrectionDown),
            (17, Parameters.AutoRegulationStep),
            (18, (int)(Parameters.AutoRegulationDelayPhase1 / 100)),
            (19, (int)(Parameters.AutoRegulationDelayPhase3 / 100))
        });

        // Prepare DB
        CurrentTest.Date = DateTime.Now;
        CurrentTest.Phase1Value = Parameters.HighVoltagePhase1;
        CurrentTest.Phase3Value = Parameters.HighVoltagePhase3;
        CurrentTest.Phase1Duration = Parameters.DurationPhase1;
        CurrentTest.Phase2Duration = Parameters.DurationPhase2;
        CurrentTest.Phase3Duration = Parameters.DurationPhase3;
        CurrentTest.LoadForce = Parameters.LoadForce;
        CurrentTest.AutoRegulation = Parameters.AutoRegulationHV;
        CurrentTest.AutoRegulationDelayPhase1 = Parameters.AutoRegulationDelayPhase1;
        CurrentTest.AutoRegulationDelayPhase3 = Parameters.AutoRegulationDelayPhase3;
        CurrentTest.AutoRegulationStep = Parameters.AutoRegulationStep;
        CurrentTest.AutoRegulationMaxCorrectionUp = Parameters.AutoRegulationMaxCorrectionUp;
        CurrentTest.AutoRegulationMaxCorrectionDown = Parameters.AutoRegulationMaxCorrectionDown;
        CurrentTest.IsPlusPolarity = !(((Parameters.Counter - 1) / Parameters.ChangePolarityStep % 2 == 0) ^
                                      Parameters.IsStartPlusPolarity);

        // Prepare Robot
        await AllDevices.RobotDevice.SetRegisterAsync(5, true);    // RESET positions
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotLoadForceRegister, Parameters.LoadForce);
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotGotoPositionRegister, 0);

        await DeadStep();
    }

    #endregion Process Cycle
}