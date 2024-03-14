using System.ComponentModel;

namespace ElAd2024.Contracts.Devices;

public interface IDevice : IDisposable, INotifyPropertyChanged
{
    Task InitializeAsync();
    bool IsSimulated { get; }
    bool IsConnected { get; }
    Task ConnectAsync(object? parameters = null);
    Task DisconnectAsync();

    Task Stop();
    Task GetData(bool forceClear = false);
}
