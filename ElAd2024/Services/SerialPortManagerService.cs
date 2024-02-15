using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.ExtensionMethods;
using ElAd2024.Models;

namespace ElAd2024.Services;

public partial class SerialPortManagerService : ObservableObject, IDisposable
{
    public delegate void SerialDataReceivedEventHandler(string data);

    private const string NewLine = "\r\n";
    private CancellationTokenSource? cancellationTokenSource;
    [ObservableProperty] private bool commandSent;
    private DataReader? dataReader;
    private DataWriter? dataWriter;

    public SerialPortInfo? PortInfo { get; set; }
    public SerialDevice? Device { get; set; }
    public uint CountBytes { get; set; } = 1024;

    public void Dispose()
    {
        CloseSerialPort();
        GC.SuppressFinalize(this);
    }

    public static async Task<List<SerialPortInfo>> GetAvailableSerialPortsAsync()
    {
        var ports = new List<SerialPortInfo>();
        var selector = SerialDevice.GetDeviceSelector();
        var devices = await DeviceInformation.FindAllAsync(selector);
        foreach (var dev in devices)
        {
            using var serialPort = await SerialDevice.FromIdAsync(dev.Id);
            if (serialPort is null)
            {
                continue;
            }

            SerialPortInfo newPort = new()
            {
                Id = dev.Id,
                Name = dev.Name,
                PortName = serialPort.PortName,
                PortNumber = byte.Parse(serialPort.PortName.Replace("COM", ""))
            };

            serialPort.CopyTo(newPort);
            ports.Add(newPort);
        }
        return ports;
    }

    public async Task WriteAsync(string? data)
    {
        ArgumentNullException.ThrowIfNull(Device, nameof(Device));
        ArgumentNullException.ThrowIfNull(dataWriter, nameof(dataWriter));
        ArgumentException.ThrowIfNullOrWhiteSpace(data, nameof(data));
        
        dataWriter?.WriteString(data + NewLine);
        await dataWriter?.StoreAsync();
        CommandSent = true;
    }


    public async Task OpenSerialPortAsync(SerialPortInfo serialPortInfo)
    {
        ArgumentNullException.ThrowIfNull(serialPortInfo);

        CloseSerialPort(); // Ensure any existing port is closed before opening a new one
        PortInfo = serialPortInfo;

        try
        {
            Device = await SerialDevice.FromIdAsync(PortInfo?.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening serial port: {ex.Message}");
            // Handle the error appropriately
        }

        if (Device is not null)
        {
            PortInfo?.CopyTo(Device);

            dataWriter = new DataWriter(Device.OutputStream);
            dataReader = new DataReader(Device.InputStream) { InputStreamOptions = InputStreamOptions.Partial };

            cancellationTokenSource = new CancellationTokenSource();
            _ = StartReadingAsync(Device, cancellationTokenSource.Token);
        }
        else
        {
            // Log or handle the case where the serial device could not be opened
            Debug.WriteLine($"Unable to open serial port: {PortInfo?.PortName}");
        }
    }

    public event SerialDataReceivedEventHandler? DataReceived;

    protected virtual void OnDataReceived(string data)
        => DataReceived?.Invoke(data);
    
    private async Task StartReadingAsync(SerialDevice? serialDevice, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serialDevice);
        ArgumentNullException.ThrowIfNull(dataReader);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await dataReader.LoadAsync(CountBytes).AsTask(cancellationToken);
                if (bytesRead > 0)
                {
                    var receivedData = dataReader.ReadString(bytesRead);
                    OnDataReceived(receivedData);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Handle the cancellation
            Debug.WriteLine("Read operation was cancelled.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ReadAsync: Exception: " + ex.Message);
            // Handle other exceptions if necessary
        }

        CommandSent = false;
    }

    public void CloseSerialPort()
    {
        try
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CloseSerialPort: cancellationTokenSource Exception: " + ex.Message);
        }

        try
        {
            dataWriter?.DetachStream();
            dataWriter?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CloseSerialPort: dataWriter Exception: " + ex.Message);
        }

        dataWriter = null;

        try
        {
            dataReader?.DetachStream();
            dataReader?.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("CloseSerialPort: dataReader Exception: " + ex.Message);
        }

        dataReader = null;

        Device?.Dispose();
        Device = null;
        PortInfo = null;
    }
}