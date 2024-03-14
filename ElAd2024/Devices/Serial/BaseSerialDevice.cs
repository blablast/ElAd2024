using System.Diagnostics;
using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using ElAd2024.Models;
using ElAd2024.Services;

namespace ElAd2024.Devices.Serial;

public partial class BaseSerialDevice : ObservableRecipient, IDevice
{
    protected readonly SerialPortManagerService DeviceService = new();

    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private bool isSimulated = false;
    public SerialPortInfo? PortInfo
    {
        get;
        set;
    }


    public virtual Task InitializeAsync() => throw new NotImplementedException();

    public async Task ConnectAsync(object? parameters)
    {
        if (IsConnected)
        {
            await DisconnectAsync();
        }

        if (parameters is SerialPortInfo portInfo)
        {
            PortInfo = portInfo;
        }
        else
        {
            throw new ArgumentException("Invalid parameters type.", nameof(parameters));
        }

        await OnConnecting();
        await DeviceService.OpenSerialPortAsync(PortInfo);
        if (DeviceService.Device == null)
        {
            Debug.WriteLine($"Failed to connect to {PortInfo.Name}.");
            return;
        }

        DeviceService.DataReceived += OnDataReceived;
        IsConnected = true;
        await OnConnected();
        await Stop();
    }

    public async Task DisconnectAsync()
    {
        await OnDisconnecting();
        DeviceService.DataReceived -= OnDataReceived;
        DeviceService.CloseSerialPort();
        IsConnected = false;
        await OnDisconnected();
    }

    // Implement IDisposable to cleanup resources.
    public async void Dispose()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }

    protected async virtual Task SendDataAsync(string data)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        }
        if (IsConnected && !string.IsNullOrEmpty(data))
        {
            await DeviceService.WriteAsync(data);
        }
    }

    protected virtual Task OnConnecting() => Task.CompletedTask; // Hook for derived classes.
    protected virtual Task OnConnected() => Task.CompletedTask; // Hook for derived classes.
    public virtual Task Stop() => Task.CompletedTask; // Hook for derived classes.
    protected virtual Task OnDisconnecting() => Task.CompletedTask; // Hook for derived classes.
    protected virtual Task OnDisconnected() => Task.CompletedTask; // Hook for derived classes.


    private void OnDataReceived(string data)
    {
        // Process each non-empty line of received data.
        foreach (var line in data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            ProcessDataLine(line);
        }
    }

    // Override in derived classes to process each line of data.
    protected virtual void ProcessDataLine(string dataLine) => Debug.WriteLine($"Received data line: {dataLine}");
    public Task GetData(bool forceClear = false) => throw new NotImplementedException();
}