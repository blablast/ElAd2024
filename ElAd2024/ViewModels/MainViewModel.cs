using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Devices;
using ElAd2024.Contracts.Services;
using ElAd2024.Models;
using ElAd2024.Models.Database;
using ElAd2024.Services;
using ElAd2024.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ElAd2024.ViewModels;

public partial class MainViewModel : ObservableRecipient, IDisposable
{
    #region Fields
    // Dependency Injections
    private readonly IDatabaseService db;
    private readonly ILocalSettingsService localSettingsService;

    private XamlRoot? xamlRoot;
    private readonly DispatcherQueue? dispatcherQueue;
    private readonly Timer scaleTimer;
    private Test testInProgress = new();

    #endregion

    #region Properties

    [ObservableProperty] private Batch? selected;
    partial void OnSelectedChanged(Batch? oldValue, Batch? newValue)
    {
        IsSelected = newValue is not null;
        SelectedFullName =
            newValue == null
                ? string.Empty
                : $"{newValue.Name}, Fabric: {newValue.FabricType}{newValue.FabricGSM}g, {newValue.FabricComposition}, {newValue.FabricColor}";
        SelectedAlgorithm = Algorithms.FirstOrDefault() ?? new Algorithm();
    }

    [ObservableProperty] private string selectedFullName = "<Select a batch>";
    [ObservableProperty] private bool isSelected;
    [ObservableProperty] private bool isTesting;

    [ObservableProperty] private TestParameters parameters = new();
    [ObservableProperty] private ProceedTestService proceedTest;
    [ObservableProperty] private IAllDevices allDevices;
    public ObservableCollection<Algorithm> Algorithms { get; set; }
    [ObservableProperty] private Algorithm? selectedAlgorithm;

    async partial void OnSelectedAlgorithmChanged(Algorithm? value)
    {
        if (value is not null)
        {
            await ProceedTest.InitializeStepsAsync(value.Id);
        }
    }

    public ObservableCollection<Batch> Batches { get; set; }

    // Devices
    public ObservableCollection<SerialPortInfo> AvailablePorts { get; } = [];

    #endregion Properties

    #region Dispose
    public void Dispose()
    {
        if (ProceedTest is not null)
        {
            ProceedTest.PropertyChanged -= ProceedTest_PropertyChanged;
            ProceedTest.Dispose();
        }
        scaleTimer?.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

    #region Initialization

    public MainViewModel(ILocalSettingsService localSettingsService, IDatabaseService databaseService, IAllDevices allDevices)
    {
        AllDevices = allDevices;
        this.localSettingsService = localSettingsService;
        db = databaseService;
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        Parameters = localSettingsService.Parameters;
        if (Parameters is null)
        {

            Parameters = new TestParameters();
            localSettingsService.Parameters = Parameters;
        }
        else
        {
            if (Parameters.HighVoltagePhase1Hi == TestParameters.MinHighVoltage)
            {
                Parameters.HighVoltagePhase1Hi = TestParameters.MaxHighVoltage;
            }

            if (Parameters.HighVoltagePhase3Hi == TestParameters.MinHighVoltage)
            {
                Parameters.HighVoltagePhase3Hi = TestParameters.MaxHighVoltage;
            }
        }
        Parameters.Counter = 0;

        Batches = new ObservableCollection<Batch>(db
            .Batches
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
        );
        Algorithms = new ObservableCollection<Algorithm>(db.Algorithms!);

        ProceedTest = App.GetService<ProceedTestService>();
        ProceedTest.PropertyChanged += ProceedTest_PropertyChanged;

        scaleTimer = new Timer(ScaleTimerCallback, null, Timeout.Infinite, 0);
    }

    private void ScaleTimerCallback(object? state)
        => dispatcherQueue?.TryEnqueue(async () =>
        {
            await AllDevices.ScaleDevice.GetWeight();
        });

    public async Task InitializeAsync(XamlRoot xamlRoot)
    {
        this.xamlRoot = xamlRoot;
        scaleTimer.Change(0, AllDevices.ScaleDevice.IsSimulated ? 5000 : 100);
        await Task.CompletedTask;
    }


    private async void ProceedTest_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ProceedTest.CurrentStep):
                return;
            case nameof(ProceedTest.IsRunning) when !ProceedTest.IsRunning:
                {
                    await db.Context.SaveChangesAsync();
                    if (Parameters.Counter < Parameters.Total)
                    {
                        await RunNextTest();
                    }
                    else
                    {
                        IsTesting = false;
                    }
                    break;
                }
        }
    }

    private async Task RunNextTest()
    {
        await Task.Delay(100);  // Must be here to avoid running DeadStep before the next test
        ArgumentNullException.ThrowIfNull(Selected);
        ArgumentNullException.ThrowIfNull(SelectedAlgorithm);

        if (ProceedTest?.IsRunning == true)
        {
            return;
        }

        IsTesting = true;
        Parameters.Counter++;
        //Setup Parameterrs for next test
        Parameters.HighVoltagePhase1 = GetValueForNextTest(Parameters.Counter, Parameters.Total,
            Parameters.HighVoltagePhase1Lo, Parameters.HighVoltagePhase1Hi, Parameters.HighVoltagePhase1MaxToMin);
        Parameters.HighVoltagePhase3 = GetValueForNextTest(Parameters.Counter, Parameters.Total,
            Parameters.HighVoltagePhase3Lo, Parameters.HighVoltagePhase3Hi, Parameters.HighVoltagePhase3MaxToMin);
        Parameters.DurationPhase1 = GetValueForNextTest(Parameters.Counter, Parameters.Total,
            Parameters.DurationPhase1Lo, Parameters.DurationPhase1Hi, Parameters.DurationPhase1MaxToMin);
        Parameters.DurationPhase2 = GetValueForNextTest(Parameters.Counter, Parameters.Total,
            Parameters.DurationPhase2Lo, Parameters.DurationPhase2Hi, Parameters.DurationPhase2MaxToMin);
        Parameters.DurationPhase3 = GetValueForNextTest(Parameters.Counter, Parameters.Total,
            Parameters.DurationPhase3Lo, Parameters.DurationPhase3Hi, Parameters.DurationPhase3MaxToMin);

        await localSettingsService.SaveParametersAsync();

        testInProgress = new Test();
        // Add new test to batches ni db
        Selected.Tests ??= [];
        Selected.Tests.Add(testInProgress);
        await db.Context.SaveChangesAsync();

        //Setup Parameterrs for next test
        ArgumentNullException.ThrowIfNull(ProceedTest);

        Debug.WriteLine($"Setting parameters for test.");
        ProceedTest!.CurrentTest = testInProgress;
        ProceedTest.Parameters = Parameters;
        ProceedTest.AllDevices.PadDevice!.Voltages.Clear();

        Debug.WriteLine($"Starting test {Parameters.Counter} of {Parameters.Total}");
        await ProceedTest.StartTest();
        return;

        static int GetValueForNextTest(int counter, int total, int min, int max, bool maxToMin)
        {
            var step = (counter - 1) * (max - min) / Math.Max(1, total - 1);
            return maxToMin ? max - step : min + step;
        }
    }

    #endregion

    #region RelayCommands

    // RelayCommands

    [RelayCommand]
    public async Task PadStartAsync()
    {
        ArgumentNullException.ThrowIfNull(AllDevices.PadDevice);
        await AllDevices.PadDevice.StartCycle(Parameters.IsStartPlusPolarity);
    }


    [RelayCommand]
    public async Task PadStopAsync()
    {
        ArgumentNullException.ThrowIfNull(AllDevices.PadDevice);
        await AllDevices.PadDevice.StopCycle(false);
    }

    [RelayCommand]
    public async Task StartTests()
    {
        if (xamlRoot is not null)
        {
            Parameters.Counter = 0;
            if (CanStartTests() && await CustomContentDialog.ShowYesNoQuestionAsync(xamlRoot, "Run tests",
                    $"Do you want to do {Parameters.Total - Parameters.Counter} tests?"))
            {
                if (SelectedAlgorithm is not null)
                {
                    await ProceedTest.InitializeStepsAsync(SelectedAlgorithm.Id);
                    await RunNextTest();
                }
            }
        }
        else
        {
            await CustomContentDialog.ShowInfoAsync(xamlRoot, "Error",
                $"Please check if all devices are connected and selected batch is not null, and weight is not 0!\nxamlRoot: {xamlRoot is not null}\nSelected: {Selected is not null}");
        }
    }

    private bool CanStartTests()
        => Selected is not null
           && (!ProceedTest?.IsRunning ?? false)
           && Parameters.Total > Parameters.Counter
           && AllDevices.PadDevice.IsConnected == true
           && (AllDevices.ScaleDevice.IsConnected == true)
           && (AllDevices.TemperatureDevice.IsConnected == true)
           && (AllDevices.HumidityDevice.IsConnected == true)
           && (AllDevices.MediaDevice.IsConnected == true || AllDevices.MediaDevice.IsConnected == false)
           && (AllDevices.RobotDevice.IsConnected == true);

    // Scale Commands
    [RelayCommand]
    public async Task ScaleGetWeightAsync()
    {
        ArgumentNullException.ThrowIfNull(AllDevices.ScaleDevice);
        await AllDevices.ScaleDevice.GetWeight();
    }

    [RelayCommand]
    public async Task ScaleTareAsync()
    {
        ArgumentNullException.ThrowIfNull(AllDevices.ScaleDevice);
        await AllDevices.ScaleDevice.Tare();
    }

    [RelayCommand]
    public async Task ScaleZeroAsync()
    {
        ArgumentNullException.ThrowIfNull(AllDevices.ScaleDevice);
        await AllDevices.ScaleDevice.Zero();
    }

    [RelayCommand]
    public async Task TestDrive()
    {
        await AllDevices.MediaDevice.StartRecording("name");
        await Task.Delay(5000);
        await AllDevices.MediaDevice.StopRecording();

    }

    [RelayCommand]
    public async Task SaveParameters()
    {
        await localSettingsService.SaveParametersAsync();
        await CustomContentDialog.ShowInfoAsync(xamlRoot, "Info", $"Parameters saved!");
    }
    #endregion
}