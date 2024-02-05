using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Models;
using ElAd2024.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

namespace ElAd2024.ViewModels;

public partial class MainViewModel : ObservableRecipient, IDisposable
{
    [ObservableProperty] private TestParameters parameters = new();
    public ObservableCollectionNotifyPropertyChange<TestStep> Steps =
        [
            new("Setup:\nPad\nRobot", "", TestStep.StepType.Computer, 0),
            new("Check:\nEnviroment", "", TestStep.StepType.Enviroment, 10),
            new("Move To:\nLoad+\nPosition", "", TestStep.StepType.Robot, 20),
            new ("Take\n1st photo", "", TestStep.StepType.Computer, 30),
            new("Read\n1st weight", "", TestStep.StepType.Scale, 40),
            new("TouchSkip\nLoad\nPosition", "", TestStep.StepType.Robot, 50),
            new("Charge\nFabric", "", TestStep.StepType.Pad, 60),
            new("Load\nFabric", "", TestStep.StepType.Pad, 70),
            new("Move To\nLoad+\nPosition", "", TestStep.StepType.Robot, 80),
            new("Read\n2nd weight", "", TestStep.StepType.Scale, 90),
            new("Move To\nObserving\nPosition", "", TestStep.StepType.Robot, 100),
            new("Take\n2nd photo", "", TestStep.StepType.Computer, 110),
            new("Observing\nFabric", "", TestStep.StepType.Pad, 120),
            new("Read\n3rd weight", "", TestStep.StepType.Scale, 130),
            new("Move To\nUnload+\nPosition", "", TestStep.StepType.Robot, 140),
            new("TouchSkip\nUnload\nPosition", "", TestStep.StepType.Robot, 150),
            new("Release\nFabric", "", TestStep.StepType.Pad, 160),
            new("Move To\nUnload+\nPosition", "", TestStep.StepType.Robot, 170),
            new("Move To\nObserving\nPosition", "", TestStep.StepType.Robot, 180),
        ];

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
    public ObservableCollection<Batch> Batches { get; set; }

    private Test testData = new();

    private XamlRoot? xamlRoot;


    // Devices
    public ObservableCollection<SerialPortInfo> AvailablePorts { get; private set; } = [];

    private byte envCountDataReceived = 0;
    [ObservableProperty] private EnvDataViewModel envDevice;
    partial void OnEnvDeviceChanging(EnvDataViewModel? oldValue, EnvDataViewModel newValue)
    {
        if (oldValue is not null) { oldValue.PropertyChanged -= EnvDevice_PropertyChanged; }
        if (newValue is not null) {
            envCountDataReceived = 0;
            newValue.PropertyChanged += EnvDevice_PropertyChanged; }
    }
    private void EnvDevice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EnvDevice.Humidity) || e.PropertyName == nameof(EnvDevice.Temperature))
        {
            envCountDataReceived++;

            if(envCountDataReceived == 2)
            {
                Debug.WriteLine($"Humidity: {EnvDevice.Humidity}, Temperature: {EnvDevice.Temperature}");
                testData.Temperature = EnvDevice.Temperature;
                testData.Humidity = EnvDevice.Humidity;
                envCountDataReceived = 0;
                EnvDevice.PropertyChanged -= EnvDevice_PropertyChanged;
            }
        }
    }


    [ObservableProperty] private PadDataViewModel padDevice;
    partial void OnPadDeviceChanging(PadDataViewModel? oldValue, PadDataViewModel newValue)
    {
        if (oldValue is not null) { oldValue.PropertyChanged -= PadDevice_PropertyChanged; }
        if (newValue is not null) { newValue.PropertyChanged += PadDevice_PropertyChanged; }
    }
    private void PadDevice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PadDataViewModel.PhaseNumber) && PadDevice.PhaseNumber == 4)
        {
            // Handle the change in PhaseNumber here
            Debug.WriteLine($"Start Moving!");
        }
    }
    [ObservableProperty] private ScaleDataViewModel scaleDevice;
    [ObservableProperty] private CameraService cameraDevice;
    [ObservableProperty] private RobotService? robotDevice;


    #region Process Cycle

    private async Task SetupPadAndRobot()
    {
        List<(int, uint)> setupList =
        [
            (1, Parameters.HighVoltagePhase1),
            (2, Parameters.HighVoltagePhase3),
            (4, Parameters.DurationPhase1 / 100),
            (5, Parameters.DurationPhase2 / 100),
            (6, Parameters.DurationPhase3 / 100),
            (7, (uint)(Parameters.AutoRegulationHV ? 1 : 0)),
        ];
        await PadDevice.Setup(setupList);
    }


    #endregion



    // Dependency Injections
    private readonly IDatabaseService db;
    private readonly ILocalSettingsService localSettingsService;

    #region Initialization

#pragma warning disable CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
    public MainViewModel(ILocalSettingsService localSettingsService, IDatabaseService databaseService)
#pragma warning restore CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
    {
        db = databaseService;

        this.localSettingsService = localSettingsService;
        Batches = new ObservableCollection<Batch>(db.Batches.Include(batch => batch.Tests));
        CameraDevice = new();
        Steps.Start(OnStepChanged);
    }

    private void OnStepChanged(object? sender, PropertyChangedEventArgs e) => Steps?.ForceRefresh(sender);

    public async Task InitializeAsync(XamlRoot xamlRoot)
    {
        this.xamlRoot = xamlRoot;
        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailablePorts.Add(port));

        EnvDevice = new(await LoadSettingAsync("EnvironmentalSensorsSettings"));
        if (EnvDevice is not null) { await EnvDevice.ConnectAsync(); }

        ScaleDevice = new(await LoadSettingAsync("ScaleDeviceSettings"));
        if (ScaleDevice is not null)
        {
            await ScaleDevice.ConnectAsync();
            StartTimer();
        }

        PadDevice = new(await LoadSettingAsync("PadDeviceSettings"));
        if (PadDevice is not null) { await PadDevice.ConnectAsync(); }

        var robotIp = await localSettingsService.ReadSettingAsync<string>("IPAddress");
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

    private async Task<SerialPortInfo> LoadSettingAsync(string key)
    {
        var setting = await localSettingsService.ReadSettingAsync<SerialPortInfo>(key);
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
    #endregion

    #region Scale Timer
    // Add event every 2 sec.
    private Timer timer;

    private async void TimerCallback(object? state)
    {
        // Your repeated logic here
        await ScaleDevice.GetWeight();
    }

    // Start the timer
    private void StartTimer(int dueTime = 0, int period = 100)
    {
        if (timer == null)
        {
            timer = new Timer(TimerCallback, null, dueTime, period);
        }
        else
        {
            timer.Change(dueTime, period);
        }
    }

    // Stop the timer
    private void StopTimer()
        => timer?.Change(Timeout.Infinite, 0);

    #endregion

    #region RelayCommands

    // RelayCommands

    [RelayCommand]
    public async Task PadStartAsync()
    {
        await PadDevice.StartCycle();
        //await CameraService.StartRecording();
        var photoName = await CameraDevice.CapturePhoto();
        Debug.WriteLine($"Photo: {photoName}");
    }


    private int prev = 1;
    [RelayCommand]
    public async Task PadStopAsync()
    {
        await PadDevice.StopCycle();
        //await CameraService.StopRecording();

        Random random = new();
        Steps[prev].IsFrozen = true;
        prev = random.Next(0, Steps.Count);
        Steps[prev].IsFrozen = false;
    }


    // Scale Commands
    [RelayCommand]
    public async Task ScaleGetWeightAsync() => await ScaleDevice.GetWeight();
    [RelayCommand]
    public async Task ScaleTareAsync() => await ScaleDevice.Tare();
    [RelayCommand]
    public async Task ScaleZeroAsync() => await ScaleDevice.Zero();


    #endregion

    #region Dispose
    public async void Dispose()
    {
        Steps?.Stop();

        StopTimer();
        if (ScaleDevice?.IsConnected ?? false)
        {
            await ScaleDevice.DisconnectAsync();
        }
        ScaleDevice?.Dispose();
        if (PadDevice?.IsConnected ?? false)
        {
            await PadDevice.DisconnectAsync();
        }
        PadDevice?.Dispose();
        if (EnvDevice?.IsConnected ?? false)
        {
            await EnvDevice.DisconnectAsync();
        }
        EnvDevice?.Dispose();

        timer?.Dispose();

        GC.SuppressFinalize(this);
    }
    #endregion
}