using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Devices;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers;
using ElAd2024.Helpers.General;
using ElAd2024.Models;
using ElAd2024.Services;
using ElAd2024.Views.Dialogs;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace ElAd2024.ViewModels;

public partial class SettingsViewModel : ObservableRecipient, IDisposable
{
    #region Fields
    private readonly IThemeSelectorService themeSelectorService;
    private readonly DispatcherQueue dispatcherQueue;

    private readonly Timer timerAvailablePorts;
    #endregion

    #region Properties
    public ILocalSettingsService LocalSettingsService
    {
        get; private set;
    }
    private readonly IDatabaseService databaseService;


    [ObservableProperty] private string selectedEnvDevicePort = string.Empty;
    partial void OnSelectedEnvDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.EnvDeviceSettings, value);

    [ObservableProperty] private string selectedScaleDevicePort = string.Empty;
    partial void OnSelectedScaleDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.ScaleDeviceSettings, value);

    [ObservableProperty] private string selectedPadDevicePort = string.Empty;
    partial void OnSelectedPadDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.PadDeviceSettings, value);

    [ObservableProperty] private string selectedElectricFieldDevicePort = string.Empty;
    partial void OnSelectedElectricFieldDevicePortChanged(string value)
        => AssignPortName(LocalSettingsService.ElectricFieldDeviceSettings, value);

    [ObservableProperty] private Microsoft.UI.Xaml.ElementTheme elementTheme;
    [ObservableProperty] private string versionDescription;
    [ObservableProperty] private IAllDevices allDevices;
    public ObservableCollection<string?> AvailableSerialPorts { get; set; } = [];
    #endregion

    #region Constructors
    public SettingsViewModel(ILocalSettingsService localSettingsService, IThemeSelectorService themeSelectorService, IDatabaseService databaseService, IAllDevices allDevices)
    {
        AllDevices = allDevices;
        this.themeSelectorService = themeSelectorService;
        LocalSettingsService = localSettingsService;
        this.databaseService = databaseService;

        dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        ElementTheme = this.themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();
        timerAvailablePorts = new Timer(LoadAvailableSerialPortsAndInitializeSettings, null, 0, 5000);
    }

    private async void LoadAvailableSerialPortsAndInitializeSettings(object? state)
    {
        dispatcherQueue?.TryEnqueue(async () => { await LoadAvailableSerialPortsAndInitializeSettings(); });
        await Task.CompletedTask;
    }
    #endregion

    #region Public Methods
    public async Task InitializeAsync()
    {
        await LoadAvailableSerialPortsAndInitializeSettings();
        await Task.CompletedTask;
    }
    #endregion

    #region Private Methods
    private static void AssignPortName(SerialPortInfo? value, string selected)
    {
        if (value is not null)
        {
            value.PortName = selected?[..(selected.IndexOf(':'))];
            value.Name = selected?[(selected.IndexOf(':') + 2)..];
        }
    }
    private async Task LoadAvailableSerialPortsAndInitializeSettings()
    {
        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailableSerialPorts.Add(port.ToString()));
        SelectedEnvDevicePort = LocalSettingsService.EnvDeviceSettings.ToString();
        SelectedScaleDevicePort = LocalSettingsService.ScaleDeviceSettings.ToString();
        SelectedPadDevicePort = LocalSettingsService.PadDeviceSettings.ToString();
        SelectedElectricFieldDevicePort = LocalSettingsService.ElectricFieldDeviceSettings.ToString();
    }

    private static string GetVersionDescription()
    {
        Version version;
        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
        }
        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    #endregion

    #region Commands

    [RelayCommand]
    public async Task SwitchThemeAsync(Microsoft.UI.Xaml.ElementTheme param)
    {
        if (ElementTheme != param)
        {
            ElementTheme = param;
            await themeSelectorService.SetThemeAsync(param);
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        await LocalSettingsService.SaveSerialsAsync();
        await App.GetService<ILocalSettingsService>().InitializeAsync();
        await App.GetService<IAllDevices>().InitializeAsync();
    }

    [RelayCommand]
    private async Task ResetSettings()
    {
        if (await Dialogs.ShowYesNoQuestionAsync("Delete Database", $"Do you really want to remove database and recreate it (all data will be lost)?"))
        {
            await databaseService.Context.Database.EnsureDeletedAsync();
            await databaseService.Context.Database.EnsureCreatedAsync();
            //await localSettingsService.ResetAsync();
        }
    }

    [RelayCommand]
    private async Task OpenDBFolder()
    {
        await FilesAndFolders.OpenFolderAsync(LocalSettingsService.ApplicationDataFolder);
    }

    public void Dispose()
    {
        timerAvailablePorts.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion

}
