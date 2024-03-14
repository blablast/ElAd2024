using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;

namespace ElAd2024.Devices.Simulator;

public partial class BaseSimulator : ObservableRecipient, IDevice, INotifyPropertyChanged
{
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private bool isSimulated = true;
    public async virtual Task InitializeAsync()
    {
        await Task.CompletedTask;
    }

    public async virtual Task ConnectAsync(object? parameters)
    {
        IsConnected = true;
        await Task.CompletedTask;
    }

    public async virtual Task DisconnectAsync()
    {
        IsConnected = false;
        await Task.CompletedTask;
    }

    // Implement IDisposable to cleanup resources.
    public async virtual void Dispose()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }
    public Task GetData(bool forceClear = false) => throw new NotImplementedException();
    public Task Stop() => throw new NotImplementedException();
}