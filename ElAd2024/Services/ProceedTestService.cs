using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.ViewModels;
using Microsoft.UI.Dispatching;
namespace ElAd2024.Services;
public partial class ProceedTestService : ObservableRecipient, IDisposable
{
    public enum ErrorType
    {
        None,
        WeightIsZero,
    }


    #region Fields

    private readonly int robotTimerPeriod = 100;
    private readonly DispatcherQueue dispatcherQueue;
    private readonly Timer robotTimer;
    private readonly Timer awaitPadTimer;
    private readonly ILocalSettingsService localSettingsService;


    private readonly bool robotSimulation = true;

    #endregion


    #region Properties

    [ObservableProperty] private bool isRunning;
    [ObservableProperty] private ErrorType error;
    [ObservableProperty] private Test? testObject;
    [ObservableProperty] private TestParameters? parameters;

    // Collection of test steps
    public ObservableCollectionNotifyPropertyChange<TestStep> Steps =
        [
            new ("Setup:\nPad\nRobot", "", TestStep.StepType.Computer, 0),
            new ("Check:\nEnviroment", "", TestStep.StepType.Enviroment, 10),
            new ("Move To:\nLoad+\nPosition", "", TestStep.StepType.Robot, 20),
            new ("Take\n1st photo", "", TestStep.StepType.Computer, 30),
            new ("Read\n1st weight", "", TestStep.StepType.Scale, 40),
            new ("TouchSkip\nLoad\nPosition", "", TestStep.StepType.Robot, 50),
            new ("Charge\nFabric", "", TestStep.StepType.Pad, 60),
            new ("Load\nFabric", "", TestStep.StepType.Pad, 70),
            new ("Move To\nLoad+\nPosition", "", TestStep.StepType.Robot, 80),
            new ("Read\n2nd weight", "", TestStep.StepType.Scale, 90),
            new ("Move To\nObserving\nPosition", "", TestStep.StepType.Robot, 100),
            new ("Take\n2nd photo", "", TestStep.StepType.Computer, 110),
            new ("Observing\nFabric", "", TestStep.StepType.Pad, 120),
            new ("Read\n3rd weight", "", TestStep.StepType.Scale, 130),
            new ("Move To\nUnload+\nPosition", "", TestStep.StepType.Robot, 140),
            new ("TouchSkip\nUnload\nPosition", "", TestStep.StepType.Robot, 150),
            new ("Release\nFabric", "", TestStep.StepType.Pad, 160),
            new ("Move To\nUnload+\nPosition", "", TestStep.StepType.Robot, 170),
            new ("Move To\nObserving\nPosition", "", TestStep.StepType.Robot, 180),
        ];

    [ObservableProperty] private EnvDataViewModel? envDevice;
    [ObservableProperty] private PadDataViewModel? padDevice;
    [ObservableProperty] private ScaleDataViewModel? scaleDevice;
    [ObservableProperty] private CameraService? cameraDevice;
    [ObservableProperty] private RobotService? robotDevice;
    [ObservableProperty] private int currentStep = -1;

    #endregion


    #region Constructor

    public ProceedTestService(ILocalSettingsService localSettingsService)
    {
        this.localSettingsService = localSettingsService;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        Steps.Start(OnStepChanged);

        // Initialize timers
        robotTimer = new Timer(CheckRobotPosition, null, Timeout.Infinite, 0);
        awaitPadTimer = new Timer(AwaitPadTimerCallback, null, Timeout.Infinite, 0);
    }

    #endregion

    #region Public Methods

    public void Start()
    {
        // Validate dependencies
        ValidateDependencies();
        if (ScaleDevice is null || ScaleDevice.Weight == 0)
        {
            Error = ErrorType.WeightIsZero;
            return;
        }

        IsRunning = true;
        CurrentStep = 0;
    }

    #endregion

    #region Private Methods
    private void ValidateDependencies()
    {
        // Ensures all necessary devices and settings are available before starting
        ArgumentNullException.ThrowIfNull(TestObject, nameof(TestObject));
        ArgumentNullException.ThrowIfNull(Parameters, nameof(Parameters));

        ArgumentNullException.ThrowIfNull(CameraDevice, nameof(CameraDevice));
        ArgumentNullException.ThrowIfNull(EnvDevice, nameof(EnvDevice));
        ArgumentNullException.ThrowIfNull(PadDevice, nameof(PadDevice));
        ArgumentNullException.ThrowIfNull(RobotDevice, nameof(RobotDevice));
        ArgumentNullException.ThrowIfNull(ScaleDevice, nameof(ScaleDevice));
    }

    private void OnStepChanged(object? sender, PropertyChangedEventArgs e) => Steps?.ForceRefresh(sender);

    partial void OnPadDeviceChanging(PadDataViewModel? oldValue, PadDataViewModel? newValue)
    {
        if (oldValue is not null) { oldValue.PropertyChanged -= PadDevice_PropertyChanged; }
        if (newValue is not null) { newValue.PropertyChanged += PadDevice_PropertyChanged; }
    }
    private void PadDevice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsRunning && e.PropertyName == nameof(PadDataViewModel.PhaseNumber) && (PadDevice?.PhaseNumber == 2 || PadDevice?.PhaseNumber == 4))
        {
            dispatcherQueue?.TryEnqueue(()
                =>
            {
                DeadStep($"Done!\n{PadDevice.HighVoltage}[V]", CurrentStep);
                CurrentStep++;
            });
        }
    }

    #endregion


    #region Process Cycle

    async partial void OnCurrentStepChanged(int value)
    {
        ArgumentNullException.ThrowIfNull(TestObject, nameof(TestObject));
        ArgumentNullException.ThrowIfNull(PadDevice, nameof(PadDevice));
        ArgumentNullException.ThrowIfNull(Parameters, nameof(Parameters));

        if (!IsRunning) { return; }
        switch (value)
        {
            case 0:
                await SetupPadAndRobot();
                break;
            case 1:
                await CheckEnviroment();
                break;
            case 2:
                await RobotMoveTo(RobotService.GotoPositionType.LoadPlus);
                break;
            case 3:
                //                TestObject.Photos = new List<Photo>();
                await TakeAPhoto("Ready to Load", $"photo{TestObject.TestId:D5}_start");
                break;
            case 4:
                await GetWeight("FullStack");
                break;
            case 5:
                await RobotMoveTo(RobotService.GotoPositionType.Load);
                break;
            case 6:
                dispatcherQueue?.TryEnqueue(() => { AliveStep("Charging\nFabric..."); });
                // Calculate Polarity voltage
                await PadDevice.StartCycle(TestObject.IsPlusPolarity);
                break;
            case 7:
                dispatcherQueue?.TryEnqueue(() => { AliveStep("Loading\nFabric..."); });
                break;
            case 8:
                await RobotMoveTo(RobotService.GotoPositionType.LoadPlus);
                break;
            case 9:
                await GetWeight("StackAfterLoad");
                break;
            case 10:
                await RobotMoveTo(RobotService.GotoPositionType.Home);
                break;
            case 11:
                await TakeAPhoto("Loaded", $"photo{TestObject.TestId:D5}_load");
                break;
            case 12:
                dispatcherQueue?.TryEnqueue(() =>
                {
                    AliveStep("Observing\nFabric...");
                });
                awaitPadTimer.Change((int)Parameters.DurationObserving, 0);
                break;
            case 13:
                await GetWeight("StackAfterTime");
                break;
            case 14:
                await RobotMoveTo(RobotService.GotoPositionType.UnloadPlus);
                break;
            case 15:
                await RobotMoveTo(RobotService.GotoPositionType.Unload);
                break;
            case 16:
                await PadDevice.StopCycle();
                dispatcherQueue?.TryEnqueue(() =>
                {
                    AliveStep("Releasing\nFabric...");
                    CurrentStep++;
                });
                break;
            case 17:
                await RobotMoveTo(RobotService.GotoPositionType.UnloadPlus);
                break;
            case 18:
                await RobotMoveTo(RobotService.GotoPositionType.Home);
                break;
            case 100:
                // Save test data
                TestObject.Voltages = new List<Voltage>(PadDevice.Voltages);
                IsRunning = false;
                break;
            default:
                CurrentStep++;
                break;
        }
    }
    private void AliveStep(string message)
    {
        // Updates UI to indicate a step is currently being processed
        int? counter = (int)CurrentStep;
        Steps[counter.Value].Opacity = 1;
        Steps[counter.Value].BackContent = message;
        Steps[counter.Value].IsFrozen = false;
    }

    private void DeadStep(string message, int? counter = null)
    {
        // Marks a step as completed and updates its UI representation
        counter ??= CurrentStep;
        Steps[counter.Value].FlipSlow();
        Steps[counter.Value].BackContent = message;
    }

    private async Task SetupPadAndRobot()
    {
        // Configures the pad and robot for the test
        ArgumentNullException.ThrowIfNull(Parameters, nameof(Parameters));
        ArgumentNullException.ThrowIfNull(PadDevice, nameof(PadDevice));
        ArgumentNullException.ThrowIfNull(RobotDevice, nameof(RobotDevice));
        ArgumentNullException.ThrowIfNull(TestObject, nameof(TestObject));

        // Reset steps
        Steps.ToList().ForEach(step => step.Reset());

        // Prepare PAD
        AliveStep("Setting\nup...");
        await PadDevice.Setup([
            (1, Parameters.HighVoltagePhase1),
            (2, Parameters.HighVoltagePhase3),
            (4, Parameters.DurationPhase1 / 100),
            (5, Parameters.DurationPhase2 / 100),
            (6, Parameters.DurationPhase3 / 100),
            (8, (int)(Parameters.AutoRegulationHV ? 1 : 0)),
        ]);

        // Prepare DB
        TestObject.Date = DateTime.Now;
        TestObject.HVPhaseCharging = Parameters.HighVoltagePhase1;
        TestObject.HVPhaseLoading = Parameters.HighVoltagePhase3;
        TestObject.DurationPhaseCharging = Parameters.DurationPhase1;
        TestObject.DurationPhaseIntermediary = Parameters.DurationPhase2;
        TestObject.DurationPhaseLoading = Parameters.DurationPhase3;
        TestObject.DurationPhaseObserving = Parameters.DurationObserving;
        TestObject.LoadForce = Parameters.LoadForce;
        TestObject.AutoRegulation = Parameters.AutoRegulationHV;
        TestObject.IsPlusPolarity = !((int)((Parameters.Counter - 1) / Parameters.ChangePolarityStep) % 2 == 0 ^ Parameters.IsStartPlusPolarity);

        // Prepare Robot
        RobotDevice.SetRegister(localSettingsService.RobotLoadForceRegister, (int)Parameters.LoadForce);
        RobotDevice.SetRegister(localSettingsService.RobotGotoPositionRegister, (int)RobotService.GotoPositionType.None);
        DeadStep("Done!");
        CurrentStep++;
        await Task.CompletedTask;
    }

    private async Task CheckEnviroment()
    {
        // Checks environmental conditions before proceeding with the test

        ArgumentNullException.ThrowIfNull(TestObject, nameof(TestObject));
        ArgumentNullException.ThrowIfNull(EnvDevice, nameof(EnvDevice));

        AliveStep("Checking\nEnviroment...");
        TestObject.Temperature = EnvDevice.Temperature;
        TestObject.Humidity = EnvDevice.Humidity;
        DeadStep($"{EnvDevice.Temperature,2}°C\n{EnvDevice.Humidity,2}%");
        CurrentStep++;
        await Task.CompletedTask;
    }

    private async Task RobotMoveTo(object? position)
    {
        if (position is null || position is not RobotService.GotoPositionType)
        {
            throw new ArgumentException("Invalid position", nameof(position));
        }
        await RobotMoveTo((RobotService.GotoPositionType)position);
    }


    private async Task RobotMoveTo(RobotService.GotoPositionType position)
    {
        ArgumentNullException.ThrowIfNull(RobotDevice, nameof(RobotDevice));

        dispatcherQueue.TryEnqueue(() => { AliveStep($"Moving to\n{position}\nPosition..."); });
        RobotDevice.SetRegister(localSettingsService.RobotInPositionRegister, false);
        RobotDevice.SetRegister(localSettingsService.RobotGotoPositionRegister, (int)position);
        robotTimer.Change(0, robotTimerPeriod);
        await Task.CompletedTask;
    }

    private bool isRobotTimerOn = false;
    private void CheckRobotPosition(object? state)
    {
        ArgumentNullException.ThrowIfNull(RobotDevice, nameof(RobotDevice));

        if (!isRobotTimerOn)
        {
            isRobotTimerOn = true;
            // TODO: Remove robotSimulation
            if (robotSimulation || RobotDevice.GetFlagRegister(localSettingsService.RobotInPositionRegister))
            {
                robotTimer.Change(Timeout.Infinite, 0);
                dispatcherQueue.TryEnqueue(() =>
                {
                    DeadStep("Done !");
                    CurrentStep++;
                });
            }
            else
            {
                var xyzwprPosition = RobotDevice.CurrentPosition;
                if (xyzwprPosition.IsValid)
                {
                    dispatcherQueue?.TryEnqueue(() => { AliveStep($"Position:\nx: {xyzwprPosition.X}\ny: {xyzwprPosition.Y}\nz: {xyzwprPosition.Z}"); });
                }
            }
            isRobotTimerOn = false;
        }
    }

    private async Task<string> TakeAPhoto(string description, string photoName = "photo")
    {
        ArgumentNullException.ThrowIfNull(CameraDevice, nameof(CameraDevice));
        ArgumentNullException.ThrowIfNull(TestObject, nameof(TestObject));

        dispatcherQueue.TryEnqueue(() => { AliveStep("Taking\nPhoto..."); });
        var (fileName, fullPathWithFileName) = await CameraDevice.CapturePhoto(photoName);
        if (fileName is not null)
        {
            TestObject.Photos.Add(new Photo() { FileName = fileName, FullPathFileName = fullPathWithFileName, Description = description });
            dispatcherQueue?.TryEnqueue(() =>
            {
                Steps[CurrentStep].ImageSource = fullPathWithFileName;
                DeadStep(string.Empty);
                CurrentStep++;
            });
        }
        await Task.CompletedTask;
        return fileName ?? string.Empty;
    }

    private async Task GetWeight(string description)
    {
        ArgumentNullException.ThrowIfNull(ScaleDevice, nameof(ScaleDevice));
        ArgumentNullException.ThrowIfNull(TestObject, nameof(TestObject));

        dispatcherQueue.TryEnqueue(() => { AliveStep("Reading\nWeight..."); });
        var weight = ScaleDevice.Weight;
        if (weight is not null)
        {
            TestObject.Weights.Add(new Weight() { Value = weight.Value, Description = description });
            dispatcherQueue.TryEnqueue(() => { DeadStep($"{weight}g"); CurrentStep++; });
        }

        await Task.CompletedTask;
    }

    private void AwaitPadTimerCallback(object? state)
    {
        ArgumentNullException.ThrowIfNull(PadDevice, nameof(PadDevice));

        awaitPadTimer.Change(Timeout.Infinite, 0);
        dispatcherQueue.TryEnqueue(() =>
        {
            DeadStep("Done !");
            CurrentStep++;
            PadDevice.Paused = true;
        });
    }

    public void Dispose()
    {
        Steps?.Stop();
        robotTimer?.Dispose();
        awaitPadTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}