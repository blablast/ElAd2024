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
using Windows.Media.Core;

namespace ElAd2024.Services;

public partial class ProceedTestService : ObservableRecipient, IProceedTestService
{

    #region IDisposable

    public void Dispose()
    {

        AllDevices.PadDevice.PropertyChanged -= PadDevice_PropertyChanged;
        AlgorithmSteps?.Stop();
        robotTimer?.Dispose();
        waitTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region Fields
    private readonly ILocalSettingsService localSettingsService;
    private readonly IDatabaseService databaseService;
    private readonly DispatcherQueue dispatcherQueue;

    private readonly Timer robotTimer;
    private readonly Timer waitTimer;
    private readonly Timer videoTimer;
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
        robotTimer = new Timer(UpdateRobotPosition, null, Timeout.Infinite, 0);
        waitTimer = new Timer(WaitElapsed, null, Timeout.Infinite, 0);
        videoTimer = new Timer(VideoElapsed, null, Timeout.Infinite, 0);
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
        CurrentStep = 0;
        await Task.CompletedTask;
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
    async partial void OnCurrentStepChanged(int value)
    {

        //if (value < Steps.Count)
        if (value < AlgorithmSteps?.Count)
        {
            Func<string?, Task> method = GetMethodAsFunc(AlgorithmSteps[value].AlgorithmStep!.Step.AsyncActionName);
            CurrentTest.TestSteps.Add(
                       new TestStep
                       {
                           Step = AlgorithmSteps[value].AlgorithmStep!.Step,
                           ActionParameter = AlgorithmSteps[value].AlgorithmStep!.ActionParameter,
                           FrontName = AlgorithmSteps[value].AlgorithmStep!.FrontName
                       });
            await method(AlgorithmSteps[value].ActionParameter);
        }
    }

    private void OnAlgorithmStepChanged(object? sender, PropertyChangedEventArgs e)
        => AlgorithmSteps?.ForceRefresh(sender);

    private async void PadDevice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsRunning && e.PropertyName == nameof(AllDevices.PadDevice.Phase) && AllDevices.PadDevice.Phase == 4)
        {
            await DeadStep($"Done!\n{AllDevices.PadDevice.Voltages.LastOrDefault(v => v.Phase == AllDevices.PadDevice.Phase - 1)?.Value}[V]", CurrentStep);
        }
    }

    #endregion Private Methods

    #region Process Cycle

    private async Task AliveStep(string? message = null)
    {
        // Updates UI to indicate a step is currently being processed
        int? counter = CurrentStep;
        dispatcherQueue.TryEnqueue(() =>
        {
            AlgorithmSteps[counter.Value].Opacity = 1;
            if (message?.Length > 0)
            {
                AlgorithmSteps[counter.Value].BackName = message;
            }
            AlgorithmSteps[counter.Value].IsFrozen = false;
        });
        await Task.Delay(10);
    }

    private async void VideoElapsed(object? state)
    {
        await AllDevices.MediaDevice.StopRecording();
        videoTimer.Change(Timeout.Infinite, 0);
        await DeadStep();
    }

    private async Task GetVideo(string? obj)
    {
        if (!int.TryParse(obj, out var duration))
        {
            throw new ArgumentException("Invalid parameters in function RobotMoveTo()");
        }
        await AliveStep();
        var fileName = await AllDevices.MediaDevice.StartRecording($"{CurrentTest.Id:D5}");
        CurrentTest.Videos.Add(new Video { FileName = fileName, Description = obj ?? string.Empty });
        videoTimer.Change(duration * 1000, 0);
    }

    private async void WaitElapsed(object? state)
    {
        waitTimer.Change(Timeout.Infinite, 0);
        await DeadStep();
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
                robotTimer.Change(Timeout.Infinite, 0);
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

    private async Task DeadStep(string? message = null, int? counter = null)
    {
        dispatcherQueue.TryEnqueue(() =>
        {
            // Marks a step as completed and updates its UI representation
            counter ??= CurrentStep;
            AlgorithmSteps[counter.Value].FlipSlow();
            AlgorithmSteps[counter.Value].BackName = message ?? $"{AlgorithmSteps[counter.Value].BackName.Replace("...", ".")}\nDone!";
            if (CurrentStep < AlgorithmSteps.Count - 1)
            {
                CurrentStep++;
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
        await DeadStep(string.Empty);
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

    private async Task Wait(string? _)
    {
        if (int.TryParse(_, out var duration) && duration > 0)
        {
            await AliveStep($"{duration / 1000:F1}s.");
            waitTimer.Change(duration, 0);
        }
        else
        {
            throw new ArgumentException("Invalid parameters in function ObserveFabric()");
        }
    }

    private async Task ReleaseFabric(string? _)
    {
        await AliveStep();
        await AllDevices.PadDevice.StopCycle(true);
        await DeadStep();
    }

    private async Task RobotCommand(int position, bool touchSkip)
    {
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotIsTouchSkipRegister, touchSkip);
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotGotoPositionRegister, position);
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotInPositionRegister, false);
        await AllDevices.RobotDevice.SetRegisterAsync(localSettingsService.RobotRunRegister, true);
        robotTimer.Change(0, robotTimerPeriod);
    }

    private async Task RobotMoveTo(string? obj)
    {
        Debug.WriteLine($"RobotMoveTo({obj})");
        if (!int.TryParse(obj, out var position))
        {
            throw new ArgumentException("Invalid parameters in function RobotMoveTo()");
        }
        await AliveStep($"Moving to\n{position}...");
        await RobotCommand(position, false);
    }

    private async Task RobotTouchSkip(string? obj)
    {
        Debug.WriteLine($"RobotTouchSkip({obj})");
        if (!int.TryParse(obj, out var position))
        {
            throw new ArgumentException("Invalid parameters in function RobotTouchSkip()");
        }

        await AliveStep($"Touching\nSkip to:\n{position}...");
        await RobotCommand(position, true);
    }

    private async Task Start(object? _)
    {
        AlgorithmSteps.ToList().ForEach(step => step.Reset());
        await Task.Delay(10);
        // Prepare PAD
        await AliveStep();
        await AllDevices.PadDevice.Setup(new List<(int Number, int Value)>
        {
            (1, Parameters!.HighVoltagePhase1),
            (2, Parameters.HighVoltagePhase3),
            (4, Parameters.DurationPhase1 / 100),
            (5, Parameters.DurationPhase2 / 100),
            (6, Parameters.DurationPhase3 / 100),
            (8, Parameters.AutoRegulationHV ? 1 : 0)
        });

        // Prepare DB
        CurrentTest.Date = DateTime.Now;
        CurrentTest.Phase1Value = Parameters.HighVoltagePhase1;
        CurrentTest.Phase3Value = Parameters.HighVoltagePhase3;
        CurrentTest.Phase1Duration = Parameters.DurationPhase1;
        CurrentTest.Phase2Duration = Parameters.DurationPhase2;
        CurrentTest.Phase3Duration = Parameters.DurationPhase3;
        CurrentTest.DurationPhaseObserving = Parameters.DurationObserving;
        CurrentTest.LoadForce = Parameters.LoadForce;
        CurrentTest.AutoRegulation = Parameters.AutoRegulationHV;
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