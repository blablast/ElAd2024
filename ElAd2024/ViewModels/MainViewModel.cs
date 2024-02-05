using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Services;
using ElAd2024.Models;
using ElAd2024.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

namespace ElAd2024.ViewModels;

public partial class MainViewModel : ObservableRecipient, IDisposable
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

    [ObservableProperty] private bool isStartPolarizationPlus = true;
    [ObservableProperty] private bool isSwitchWorkMode = true;
    [ObservableProperty] private int changePolarizationStep = 1;

    [ObservableProperty] private uint testsNumber = 20;


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

    [ObservableProperty] private RobotService? robotService;

    private XamlRoot? xamlRoot;

    // Add event every 2 sec.
    private Timer timer;

    [ObservableProperty] private CameraService cameraService;

    public ObservableCollection<SerialPortInfo> AvailablePorts { get; private set; } = [];

    [ObservableProperty] private bool isEnvActive;
    [ObservableProperty] private bool isPadActive;
    [ObservableProperty] private bool isScaleActive;
    [ObservableProperty] private bool isCameraActive;
    [ObservableProperty] private bool isRobotActive;


    [ObservableProperty] private EnvDataViewModel envDevice;
    [ObservableProperty] private PadDataViewModel padDevice;

    partial void OnPadDeviceChanging(PadDataViewModel? oldValue, PadDataViewModel newValue)
    {
        if (oldValue is not null) { oldValue.PropertyChanged -= PadDevice_PropertyChanged; }
        if (newValue is not null) { newValue.PropertyChanged += PadDevice_PropertyChanged; }
    }
    private void PadDevice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PadDataViewModel.PhaseNumber) && PadDevice.PhaseNumber ==4)
        {
            // Handle the change in PhaseNumber here
            Debug.WriteLine($"Start Moving!");
        }
    }


    [ObservableProperty] private ScaleDataViewModel scaleDevice;

    private readonly IDatabaseService db;
    private readonly ILocalSettingsService localSettingsService;

#pragma warning disable CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
    public MainViewModel(ILocalSettingsService localSettingsService, IDatabaseService databaseService)
#pragma warning restore CS8618 // Pole niedopuszczające wartości null musi zawierać wartość inną niż null podczas kończenia działania konstruktora. Rozważ zadeklarowanie pola jako dopuszczającego wartość null.
    {
        db = databaseService;
        
        this.localSettingsService = localSettingsService;
        Batches = new ObservableCollection<Batch>(db.Batches.Include(batch => batch.Tests));
        CameraService = new();
    }

    public async Task InitializeAsync(XamlRoot xamlRoot)
    {
        this.xamlRoot = xamlRoot;
        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailablePorts.Add(port));

        EnvDevice = new(await LoadSettingAsync("EnvironmentalSensorsSettings"));
        if (EnvDevice is not null)
        {
            IsEnvActive = true;
            await EnvDevice.ConnectAsync();
        }

        ScaleDevice = new(await LoadSettingAsync("ScaleDeviceSettings"));
        if (ScaleDevice is not null)
        {
            IsScaleActive = true;
            await ScaleDevice.ConnectAsync();
            StartTimer();
        }

        PadDevice = new(await LoadSettingAsync("PadDeviceSettings"));
        if (PadDevice is not null)
        {
            IsPadActive = true;
            await PadDevice.ConnectAsync();
        }

        var robotIp = await localSettingsService.ReadSettingAsync<string>("IPAddress");
        if (!string.IsNullOrEmpty(robotIp))
        {
            RobotService = new(robotIp);
            IsRobotActive = RobotService.Connect();
        }

        await CameraService.InitializeAsync();
        if (CameraService.AllMediaFrameSourceGroups?.Count > 0)
        {
            CameraService.SelectedMediaFrameSourceGroup = CameraService.AllMediaFrameSourceGroups[0];
            await CameraService.PreviewCamera();
            IsCameraActive = true;
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


    // RelayCommands

    [RelayCommand]
    public async Task PadStartAsync()
    {
        await PadDevice.StartCycle();
        //await CameraService.StartRecording();
        var photoName = await CameraService.CapturePhoto();
        Debug.WriteLine($"Photo: {photoName}");
    }

    [RelayCommand]
    public async Task PadStopAsync()
    {
        await PadDevice.StopCycle();
        //await CameraService.StopRecording();
    }



    // Scale Commands
    [RelayCommand]
    public async Task ScaleGetWeightAsync() => await ScaleDevice.GetWeight();
    [RelayCommand]
    public async Task ScaleTareAsync() => await ScaleDevice.Tare();
    [RelayCommand]
    public async Task ScaleZeroAsync() => await ScaleDevice.Zero();

    public async void Dispose()
    {
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
}