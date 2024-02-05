using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using System.Text;
using System.Diagnostics;
using ElAd2024.Models;
using Windows.Storage.Streams;
using System.IO.Ports;
using System.Xml.Linq;
using Windows.Networking;
using ElAd2024.ExtentionMethods;

namespace ElAd2024.Services;
public class SerialPortManagerService
{
    private const string newLine = "\r\n";
    private DataWriter? dataWriter;
    private DataReader? dataReader;

    private CancellationTokenSource? cancellationTokenSource;

    public SerialPortInfo? PortInfo { get; set; }
    public SerialDevice? Device { get; set; }
    public uint CountBytes { get; set; } = 1024;

    public static async Task<List<SerialPortInfo>> GetAvailableSerialPortsAsync()
    {
        var ports = new List<SerialPortInfo>();
        var selector = SerialDevice.GetDeviceSelector();
        var devices = await DeviceInformation.FindAllAsync(selector);
        foreach (var dev in devices)
        {
            using var serialPort = await SerialDevice.FromIdAsync(dev.Id);
            if (serialPort is not null)
            {
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
        }
        return ports;
    }

    public async Task WriteAsync(string? data)
    {
        ArgumentNullException.ThrowIfNull(Device, nameof(Device));
        ArgumentNullException.ThrowIfNull(dataWriter, nameof(dataWriter));
        ArgumentException.ThrowIfNullOrWhiteSpace(data, nameof(data));
        dataWriter?.WriteString(data + newLine);
        await dataWriter?.StoreAsync();
    }


    public async Task OpenSerialPortAsync(SerialPortInfo serialPortInfo)
    {
        CloseSerialPort(); // Ensure any existing port is closed before opening a new one

        ArgumentNullException.ThrowIfNull(serialPortInfo);

        PortInfo = serialPortInfo;
        Device = await SerialDevice.FromIdAsync(PortInfo?.Id);

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

    public delegate void SerialDataReceivedEventHandler(string data);
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
    }
    public void CloseSerialPort()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;

        dataWriter?.DetachStream();
        dataWriter?.Dispose();
        dataWriter = null;

        dataReader?.DetachStream();
        dataReader?.Dispose();
        dataReader = null;

        Device?.Dispose();
        Device = null;
        PortInfo = null;
    }
}
