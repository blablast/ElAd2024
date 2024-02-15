using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Contracts.Devices;
using ElAd2024.Contracts.Services;
using ElAd2024.Helpers.General;
using ElAd2024.Models;
using ElAd2024.Services;
using ElAd2024.Views;
using Microsoft.UI.Xaml;

namespace ElAd2024.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    #region Fields
    private readonly IThemeSelectorService themeSelectorService;
    private XamlRoot? xamlRoot;
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
        ElementTheme = this.themeSelectorService.Theme;
        VersionDescription = GetVersionDescription();
        LoadAvailableSerialPortsAndInitializeSettings();
    }
    #endregion

    #region Public Methods
    public async Task InitializeAsync(XamlRoot? xamlRoot)
    {
        this.xamlRoot = xamlRoot;
        //await themeSelectorService.InitializeAsync();
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
    private async void LoadAvailableSerialPortsAndInitializeSettings()
    {
        (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(port => AvailableSerialPorts.Add(port.ToString()));
        SelectedEnvDevicePort = LocalSettingsService.EnvDeviceSettings.ToString();
        SelectedScaleDevicePort = LocalSettingsService.ScaleDeviceSettings.ToString();
        SelectedPadDevicePort = LocalSettingsService.PadDeviceSettings.ToString();
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
        => await LocalSettingsService.SaveSerialsAsync();

    [RelayCommand]
    private async Task ResetSettings()
    {
        if (await CustomContentDialog.ShowYesNoQuestionAsync(xamlRoot, "Delete Database",
                    $"Do you really want to remove database and recreate it (all data will be lost)?"))
        {
            await databaseService.Context.Database.EnsureDeletedAsync();
            await databaseService.Context.Database.EnsureCreatedAsync();
            //await localSettingsService.ResetAsync();
        }
    }
    #endregion

}
