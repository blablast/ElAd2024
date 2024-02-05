using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.ViewModels;
using ElAd2024.Models;
using ElAd2024.Services;

namespace ElAd2024.ViewModels;
public partial class BaseSerialDataViewModel(SerialPortInfo serialPortInfo) : ObservableRecipient, IDisposable
{
    protected readonly SerialPortManagerService deviceService = new();
    public SerialPortInfo PortInfo { get; set; } = serialPortInfo ?? throw new ArgumentNullException(nameof(serialPortInfo), "SerialPortInfo cannot be null.");
    [ObservableProperty] private bool isConnected = false;
    [ObservableProperty] private string receivedData = string.Empty;

    public async Task ConnectAsync()
    {
        await OnConnecting();
        await deviceService.OpenSerialPortAsync(PortInfo);
        if (deviceService.Device == null)
        {
            Debug.WriteLine($"Failed to connect to {PortInfo.Name}.");
            return;
        }
        deviceService.DataReceived += OnDataReceived;
        IsConnected = true;
        await OnConnected();

    }
    public async Task DisconnectAsync()
    {
        await OnDisconnecting();
        deviceService.CloseSerialPort();
        deviceService.DataReceived -= OnDataReceived;
        IsConnected = false;
        await OnDisconnected();
    }

    protected async Task SendDataAsync(string data)
    {
        if (IsConnected && !string.IsNullOrEmpty(data))
        {
            await deviceService.WriteAsync(data);
        }
    }

    protected virtual Task OnConnecting() => Task.CompletedTask; // Hook for derived classes.
    protected virtual Task OnConnected() => Task.CompletedTask; // Hook for derived classes.
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
    protected virtual void ProcessDataLine(string dataLine)
    {
        Debug.WriteLine($"Received data line: {dataLine}");
    }

    // Implement IDisposable to cleanup resources.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Asynchronously disconnect to cleanup resources.
            DisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}