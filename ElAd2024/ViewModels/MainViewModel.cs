using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElAd2024.Models;
using ElAd2024.Services;
using ElAd2024.Helpers;
using System.Globalization;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using ElAd2024.Core.Models;
using Microsoft.UI.Dispatching;
using Windows.Graphics.Display;
using System.Drawing;

namespace ElAd2024.ViewModels;

public partial class MainViewModel : ObservableRecipient, IDisposable
{
    public ObservableCollection<TemperatureHumidityChartData> TemperatureHumidityChartDataCollection { get; set; } = new();


    [ObservableProperty] private float temperature;
    [ObservableProperty] private Brush temperatureBrush = new SolidColorBrush(Colors.Red);
    [ObservableProperty] private float humidity;
    [ObservableProperty] private Brush humidityBrush = new SolidColorBrush(Colors.Red);


    public ObservableCollection<SerialPortInfo> AvailablePorts
    {
        get; private set;
    }
    private SerialPortManagerService? arduino;

    [ObservableProperty]
    private SerialPortInfo? selectedSerialPortInfo;
    partial void OnSelectedSerialPortInfoChanged(SerialPortInfo? oldValue, SerialPortInfo? newValue)
    {
        Debug.WriteLine($"SelectedSerialPortInfo changed from {oldValue} to {newValue}");
        NotifyCanExecute();
    }

    [ObservableProperty] private string textToSend = string.Empty;
    [ObservableProperty] private string receivedText = string.Empty;


    private void NotifyCanExecute()
    {
        ConnectArduinoCommand.NotifyCanExecuteChanged();
        DisconnectArduinoCommand.NotifyCanExecuteChanged();
        SendDataCommand.NotifyCanExecuteChanged();
    }

    public MainViewModel()
    {
        //Services.RobotService robotService = new("192.168.1.100");
        //robotService.Connect();
        //robotService.GetFanucVisions();
        //for (var i = 1; i < 3; i++)
        //{
        //    Debug.WriteLine($"Flag #{i}: {robotService.GetFlagRegister(i)}");
        //}
        //for (var i = 196; i < 201; i++)
        //{
        //    Debug.WriteLine($"NR #{i}: {robotService.GetNumericRegister(i)}");
        //}
        //for (var i = 1; i < 3; i++)
        //{
        //    Debug.WriteLine($"SR #{i}: {robotService.GetStringRegister(i)}");
        //}

        AvailablePorts = new ObservableCollection<SerialPortInfo>();
        LoadAvailablePortsAndConnectAll();
    }

    private async Task LoadAvailablePorts() 
        => (await SerialPortManagerService.GetAvailableSerialPortsAsync()).ForEach(
                port => AvailablePorts.Add(port)
            );


    // RelayCommands
    public virtual bool CanConnectToArduino() => arduino is null;
    public virtual bool IsConnectedToArduino() => arduino is not null;
    private async Task ManageArduinoConnection()
    {
        if (arduino is null && SelectedSerialPortInfo is not null)
        {
            Debug.WriteLine($"Connecting to {SelectedSerialPortInfo.PortName}");

            arduino = new SerialPortManagerService();
            SelectedSerialPortInfo.Name = "Enviroment Sensors";
            SelectedSerialPortInfo.BaudRate = 115200;
            await arduino.OpenSerialPortAsync(SelectedSerialPortInfo);
            arduino.DataReceived += Arduino_OnDataReceived;
            Thread.Sleep(2000);
            await arduino.WriteAsync("SEND AUTO ON");
        }
        else if (arduino is not null)
        {
            Debug.WriteLine($"Disconnecting from {SelectedSerialPortInfo?.PortName}");
            await arduino.WriteAsync("SEND AUTO OFF");
            arduino.CloseSerialPort();
            arduino.DataReceived -= Arduino_OnDataReceived;
            arduino = null;

        }
        NotifyCanExecute();
    }

    public async Task LoadAvailablePortsAndConnectAll()
    {
        await LoadAvailablePorts();
        if (SelectedSerialPortInfo is null && AvailablePorts.Any(port => port.PortName == "COM21"))
        {
            SelectedSerialPortInfo = AvailablePorts.First(port => port.PortName == "COM21");
        }
        await ConnectArduinoAsync();
    }


    [RelayCommand(CanExecute = "CanConnectToArduino")]
    public async Task ConnectArduinoAsync() => await ManageArduinoConnection();

    [RelayCommand(CanExecute = "IsConnectedToArduino")]
    private void DisconnectArduino() => ManageArduinoConnection().Wait();

    [RelayCommand(CanExecute = "IsConnectedToArduino")]
    private async Task SendDataAsync() => await arduino?.WriteAsync(TextToSend);

    private void Arduino_OnDataReceived(string data)
    {
        // Check if left character is A
        if (data[0] == 'A')
        {
            var parts = data.Split(',');
            if (float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var readTemperature))
            {
                Temperature = readTemperature;
                TemperatureBrush = ColorInterpolation.GetBrushBasedOnHumidity(Temperature / 50, Colors.Blue, Colors.Green, Colors.Red);
            }
            if (float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var readHumidity))
            {
                Humidity = readHumidity;
                HumidityBrush = ColorInterpolation.GetBrushBasedOnHumidity(Humidity / 100, Colors.Yellow, Colors.Green, Colors.Blue);
            }
            if (TemperatureHumidityChartDataCollection.Count > 9)
            {
                TemperatureHumidityChartDataCollection.RemoveAt(0);
            }
            TemperatureHumidityChartDataCollection.Add(new TemperatureHumidityChartData
            {
                Category = DateTime.Now,
                TemperatureValue = Temperature,
                HumidityValue = Humidity
            });
        }
        else
        {
            ReceivedText = data;
        }
    }

    public void Dispose()
    {
        if (arduino is not null)
        {
            arduino.CloseSerialPort();
            arduino.DataReceived -= Arduino_OnDataReceived;
        }
        TemperatureHumidityChartDataCollection.Clear();
        GC.SuppressFinalize(this);
    }


}
