using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using ElAd2024.Converters;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.Services;
using ElAd2024.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Telerik.Barcode;
using Telerik.UI.Xaml.Controls.DataVisualization;

namespace ElAd2024.ViewModels;

public partial class MainViewModel : ObservableRecipient, IDisposable
{
    [ObservableProperty] private ProceedTestService proceedTest;

    [ObservableProperty] private TestParameters parameters = new();
    [ObservableProperty] private bool isSelected = false;
    [ObservableProperty] private Batch? selected;
    partial void OnSelectedChanged(Batch? oldValue, Batch? newValue)
    {
        IsSelected = newValue is not null;
        SelectedFullName =
            (newValue == null)
            ? string.Empty
            : $"{newValue.Name}, Fabric: {newValue.FabricType}{newValue.FabricGSM}g, {newValue.FabricComposition}, {newValue.FabricColor}";
    }


    [ObservableProperty] private string? selectedFullName = "<Select a batch>";
    public ObservableCollection<Batch> Batches
    {
        get; set;
    }

    [ObservableProperty] private bool isTesting = false;

    private Test testInProgress = new();

    private XamlRoot? xamlRoot;


    // Devices
    public ObservableCollection<SerialPortInfo> AvailablePorts { get; private set; } = [];

    //private byte envCountDataReceived = 0;
    [ObservableProperty] private EnvDataViewModel envDevice;
    [ObservableProperty] private PadDataViewModel padDevice;
    [ObservableProperty] private ScaleDataViewModel scaleDevice;
    [ObservableProperty] private CameraService cameraDevice;
    [ObservableProperty] private RobotService? robotDevice;

    // Dependency Injections
    private readonly IDatabaseService db;
    private readonly ILocalSettingsService localSettingsService;

    private readonly Timer scaleTimer;

    private readonly DispatcherQueue? dispatcherQueue;

    #region Initialization

#pragma warning disable CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
    public MainViewModel(ILocalSettingsService localSettingsService, IDatabaseService databaseService)
#pragma warning restore CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
    {
        dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        db = databaseService;

        this.localSettingsService = localSettingsService;
        Batches = new ObservableCollection<Batch>(db
            .Batches
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
            .Include(batch => batch.Tests)
            .ThenInclude(test => test.Photos)
            );
        CameraDevice = new();
        ProceedTest = new(localSettingsService);
        scaleTimer = new Timer(ScaleTimerCallback, null, Timeout.Infinite, 0);
    }
    private void ScaleTimerCallback(object? state)
        => dispatcherQueue?.TryEnqueue(async () => { await ScaleDevice.GetWeight(); });

    public async Task InitializeAsync(XamlRoot xamlRoot)
    {
        this.xamlRoot = xamlRoot;

        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailablePorts.Add(port));

        EnvDevice = new(LoadSPI(localSettingsService.EnvDeviceSettings));
        if (EnvDevice is not null) { await EnvDevice.ConnectAsync(); }

        ScaleDevice = new(LoadSPI(localSettingsService.ScaleDeviceSettings));
        if (ScaleDevice is not null)
        {
            await ScaleDevice.ConnectAsync();
            scaleTimer.Change(0, 100);
        }

        PadDevice = new(LoadSPI(localSettingsService.PadDeviceSettings));
        if (PadDevice is not null) { await PadDevice.ConnectAsync(); }

        var robotIp = await localSettingsService.ReadSettingAsync<string>("RobotIPAddress");
        if (!string.IsNullOrEmpty(robotIp))
        {
            RobotDevice = new(robotIp);
            await RobotDevice.ConnectAsync();
            await RobotDevice.InitializeAsync();
        }

        await CameraDevice.InitializeAsync();
        if (CameraDevice.AllMediaFrameSourceGroups?.Count > 0)
        {
            CameraDevice.SelectedMediaFrameSourceGroup = CameraDevice.AllMediaFrameSourceGroups[0];
            await CameraDevice.PreviewCamera();
        }

    }

    private SerialPortInfo LoadSPI(SerialPortInfo? setting)
    {
        if (setting is not null)
        {
            try
            {
                var realPort = AvailablePorts.First(AvailablePorts => AvailablePorts.PortName == setting.PortName);
                setting.Id = realPort?.Id;
                if (realPort is null) { setting = null; }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                setting = null;
            }
        }
        return setting ?? new SerialPortInfo();
    }



    private void ProceedTest_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ProceedTest.CurrentStep))
        {
            return;
        }
        if (e.PropertyName == nameof(ProceedTest.IsRunning) && !ProceedTest.IsRunning)
        {
            ProceedTest.PropertyChanged -= ProceedTest_PropertyChanged;

            //db.Batches.Where(batch => batch.BatchId == Selected.BatchId).FirstOrDefault()?.Tests?.Add(testInProgress);
            db.Context.SaveChanges();

            if (Parameters.Counter < Parameters.Total)
            {
                // Setup Parameterrs for next test
                RunNextTest().GetAwaiter().GetResult();
            }
            else
            {
                IsTesting = false;
            }
        }
    }

    private static int GetValueForNextTest(int counter, int total, int min, int max, bool maxToMin)
    {
        Debug.Write($"{counter} / {total}: ");
        Debug.Write(maxToMin ? $"{max} -> {min} " : $"{min} -> {max}");

        var step = (max - min) / Math.Max(1, total - 1);
        var result = maxToMin ? max - (counter - 1) * step : min + (counter - 1) * step;
        
        Debug.WriteLine($" = {result}");
        return result;
    }

    private async Task RunNextTest()
    {
        ArgumentNullException.ThrowIfNull(Selected);
        if (ProceedTest?.IsRunning == true)
        {
            return;
        }

        IsTesting = true;
        Parameters.Counter++;
        //Setup Parameterrs for next test
        Debug.Write($"HighVoltagePhase1: ");
        Parameters.HighVoltagePhase1 = GetValueForNextTest(Parameters.Counter, Parameters.Total, Parameters.HighVoltagePhase1Lo, Parameters.HighVoltagePhase1Hi, Parameters.HighVoltagePhase1MaxToMin);
        
        Debug.Write($"HighVoltagePhase3: ");
        Parameters.HighVoltagePhase3 = GetValueForNextTest(Parameters.Counter, Parameters.Total, Parameters.HighVoltagePhase3Lo, Parameters.HighVoltagePhase3Hi, Parameters.HighVoltagePhase3MaxToMin);
        
        Debug.Write($"DurationPhase1: ");
        Parameters.DurationPhase1 = GetValueForNextTest(Parameters.Counter, Parameters.Total, Parameters.DurationPhase1Lo, Parameters.DurationPhase1Hi, Parameters.DurationPhase1MaxToMin);
        
        Debug.Write($"DurationPhase2: ");
        Parameters.DurationPhase2 = GetValueForNextTest(Parameters.Counter, Parameters.Total, Parameters.DurationPhase2Lo, Parameters.DurationPhase2Hi, Parameters.DurationPhase2MaxToMin);
        
        Debug.Write($"DurationPhase3: ");
        Parameters.DurationPhase3 = GetValueForNextTest(Parameters.Counter, Parameters.Total, Parameters.DurationPhase3Lo, Parameters.DurationPhase3Hi, Parameters.DurationPhase3MaxToMin);

        testInProgress = new();
        // Add new test to batches ni db
        Selected.Tests ??= [];
        Selected.Tests.Add(testInProgress);
        await db.Context.SaveChangesAsync();

        ProceedTest = new(localSettingsService)
        {
            PadDevice = PadDevice,
            ScaleDevice = ScaleDevice,
            EnvDevice = EnvDevice,
            RobotDevice = RobotDevice,
            CameraDevice = CameraDevice,
            TestObject = testInProgress,
            Parameters = Parameters
        };
        ProceedTest.PropertyChanged += ProceedTest_PropertyChanged;
        ProceedTest.Start();
    }
    #endregion

    #region RelayCommands

    // RelayCommands

    [RelayCommand]
    public async Task PadStartAsync()
    {
        await PadDevice.StartCycle(Parameters.IsStartPlusPolarity);
    }


    [RelayCommand]
    public async Task PadStopAsync()
    {
        await PadDevice.StopCycle();
    }

    [RelayCommand]
    public async Task StartTests()
    {
        if (xamlRoot is not null)
        {
            Parameters.Counter = 0;
            if (CanStartTests() && await CustomContentDialog.ShowYesNoQuestionAsync(xamlRoot, "Run tests", $"Do you want to do {Parameters.Total - Parameters.Counter} tests?"))
            {
                await RunNextTest();
            }
        }
        else
        {
            await CustomContentDialog.ShowInfoAsync(xamlRoot, "Error", $"Please check if all devices are connected and selected batch is not null, and weight is not 0!\nxamlRoot: {xamlRoot is not null}\nSelected: {Selected is not null}");
        }
    }

    private bool CanStartTests()
        => (Selected is not null)
        && (Parameters.Total > Parameters.Counter)
        && PadDevice?.IsConnected == true
        && ScaleDevice?.IsConnected == true
        && EnvDevice?.IsConnected == true
        && CameraDevice?.IsConnected == true
        && RobotDevice?.IsConnected == true;

    [RelayCommand]
    public void SimulateRobotInPosition()
        => RobotDevice?.SetRegister(localSettingsService.RobotInPositionRegister, true);

    // Scale Commands
    [RelayCommand]
    public async Task ScaleGetWeightAsync() => await ScaleDevice.GetWeight();
    [RelayCommand]
    public async Task ScaleTareAsync() => await ScaleDevice.Tare();
    [RelayCommand]
    public async Task ScaleZeroAsync() => await ScaleDevice.Zero();


    #endregion

    #region Dispose
    public void Dispose()
    {
        ProceedTest?.Dispose();
        scaleTimer?.Dispose();
        PadDevice?.Dispose();
        EnvDevice?.Dispose();
        ScaleDevice?.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}