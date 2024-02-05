using System.Diagnostics;
using System.Reflection;
using Windows.Devices.SerialCommunication;

namespace ElAd2024.Models;
public class SerialPortInfo
{
    [OmitProperty] public string? Id { get; set; }
    [OmitProperty] public string? Name { get; set; }
    [OmitProperty] public byte PortNumber { get; set; }

    public uint BaudRate { get; set; } = 115200;
    [OmitProperty] public bool BreakSignalState { get; set; } = false;
    public ushort DataBits { get; set; } = 8;
    public SerialHandshake Handshake { get; set; } = SerialHandshake.XOnXOff;
    public bool IsDataTerminalReadyEnabled { get; set; } = true;
    public bool IsRequestToSendEnabled { get; set; } = false;
    public SerialParity Parity { get; set; } = SerialParity.None;
    public string? PortName { get; set; }
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(10);
    public SerialStopBitCount StopBits { get; set; } = SerialStopBitCount.One;
    public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(10);

    public void CopyTo(SerialDevice serialDevice)
    {
        var serialPortInfoProperties = typeof(SerialPortInfo).GetProperties();
        var serialDeviceProperties = serialDevice.GetType().GetProperties();

        foreach (var spInfoProp in serialPortInfoProperties)
        {
            // Check if the property is marked with the OmitProperty attribute
            if (spInfoProp.GetCustomAttribute<OmitPropertyAttribute>() is null)
            {
                PropertyInfo? spDeviceProp = null;
                try
                {
                    spDeviceProp = serialDeviceProperties.FirstOrDefault(p => p.Name == spInfoProp.Name && p.PropertyType == spInfoProp.PropertyType && p.CanWrite);
                    spDeviceProp?.SetValue(serialDevice, spInfoProp.GetValue(this));
                }
                catch 
                {
                    Debug.WriteLine($"Failed to set {spInfoProp.Name} from {spDeviceProp?.Name}");
                }
            }
        }
    }

    public override bool Equals(object? obj)
        => obj is not null && obj is SerialPortInfo info && PortName == info.PortName;

    public override int GetHashCode()
        => PortName?.GetHashCode() ?? 0;

    public override string ToString()
        => $"{PortName}: {Name}";
}


[AttributeUsage(AttributeTargets.Property)]
public class OmitPropertyAttribute : Attribute { }