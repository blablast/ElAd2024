using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using System.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using ElAd2024.Models;
using Windows.Storage.Streams;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace ElAd2024.Services;
public class SerialPortManagerService
{
    private const string newLine = "\r\n";
    public delegate void SerialDataReceivedEventHandler(string data);
    public event SerialDataReceivedEventHandler? DataReceived;
    private CancellationTokenSource? cancellationTokenSource;


    public SerialPortInfo? PortInfo
    {
        get; set;
    }

    public SerialDevice? Device
    {
        get; set;
    }
    protected virtual void OnDataReceived(string data)
    {
        DataReceived?.Invoke(data);
    }

    public SerialPortManagerService()
    {
    }


    public static async Task<List<SerialPortInfo>> GetAvailableSerialPortsAsync()
    {
        var ports = new List<SerialPortInfo>();
        var selector = SerialDevice.GetDeviceSelector();
        var devices = await DeviceInformation.FindAllAsync(selector);
        foreach (var device in devices)
        {
            using var serialPort = await SerialDevice.FromIdAsync(device.Id);
            if (serialPort != null)
            {
                ports.Add(new SerialPortInfo
                {
                    Id = device.Id,
                    Name = device.Name,
                    PortName = serialPort.PortName,
                    PortNumber = byte.Parse(serialPort.PortName.Replace("COM", "")),
                    BaudRate = serialPort.BaudRate,
                    DataBits = serialPort.DataBits,
                    Parity = serialPort.Parity,
                    StopBits = serialPort.StopBits,
                    Handshake = serialPort.Handshake
                });
            }
        }
        return ports;
    }

    public async Task WriteAsync(string? data) => await WriteAsync(Device, data);

    public static async Task WriteAsync(SerialDevice? serialPort, string? data)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (data.Length == 0)
        {
            return;
        }

        var dataWriter = new DataWriter(serialPort.OutputStream);
        dataWriter.WriteString(data + newLine);
        await dataWriter.StoreAsync();
        dataWriter.DetachStream();
        dataWriter.Dispose();
    }

    public async Task<string> ReadAsync() => await ReadAsync(Device);

    public static async Task<string> ReadAsync(SerialDevice? serialPort)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        var dataReader = new DataReader(serialPort.InputStream);
        var stringBuilder = new StringBuilder();
        try
        {
            var bytesRead = await dataReader.LoadAsync(1024);
            while (bytesRead > 0)
            {
                stringBuilder.Append(dataReader.ReadString(bytesRead));
                bytesRead = await dataReader.LoadAsync(1024);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ReadAsync: Exception: " + ex.Message);
            // Handle exceptions if necessary
        }
        finally
        {
            dataReader.DetachStream();
            dataReader.Dispose();
        }
        return stringBuilder.ToString();
    }

    public async Task OpenSerialPortAsync(SerialPortInfo serialPortInfo)
    {
        PortInfo = serialPortInfo;
        Device = await SerialDevice.FromIdAsync(PortInfo.Id);

        if (Device != null)
        {
            Device.BaudRate = serialPortInfo.BaudRate;
            Device.DataBits = serialPortInfo.DataBits;
            Device.Parity = serialPortInfo.Parity;
            Device.StopBits = serialPortInfo.StopBits;
            Device.Handshake = serialPortInfo.Handshake;
            Device.ReadTimeout = serialPortInfo.ReadTimeout;
            Device.WriteTimeout = serialPortInfo.WriteTimeout;

            // Start the continuous reading task
            cancellationTokenSource = new CancellationTokenSource();
            _ = StartReadingAsync(Device, cancellationTokenSource.Token);
        }
    }



    public void CloseSerialPort()
    {
        if (Device != null)
        {
            cancellationTokenSource?.Cancel();
            // Optionally, reset the CancellationTokenSource if you need to start reading again later
            Device.Dispose();
            Device = null;
            PortInfo = null;
        }
    }

    private async Task StartReadingAsync(SerialDevice? serialPort, CancellationToken cancellationToken)
    {
        if (serialPort == null)
        {
            throw new ArgumentNullException(nameof(serialPort));
        }

        // Instantiate DataReader outside of the loop
        var dataReader = new DataReader(serialPort.InputStream)
        {
            InputStreamOptions = InputStreamOptions.Partial // Allows for partial reads
        };

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var bytesRead = await dataReader.LoadAsync(1024).AsTask(cancellationToken);
                    if (bytesRead > 0)
                    {
                        OnDataReceived(dataReader.ReadString(bytesRead));
                    }
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("Handle the cancellation without logging or throwing an exception.");
                    break; // Exit the loop gracefully
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
        finally
        {
            // Ensure DataReader is disposed of once the loop exits
            dataReader.DetachStream();
            dataReader.Dispose();
        }
    }


}
