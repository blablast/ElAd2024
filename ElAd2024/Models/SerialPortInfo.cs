using Windows.Devices.SerialCommunication;

namespace ElAd2024.Models;
public class SerialPortInfo
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? PortName { get; set; }
    public byte PortNumber { get; set; }

    public uint BaudRate { get; set; }
    public ushort DataBits { get; set; }
    public SerialParity Parity { get; set; }
    public SerialStopBitCount StopBits { get; set; }
    public SerialHandshake Handshake { get; set; }
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
    public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
    public override string ToString() => $"{PortName}: {Name} ({BaudRate})";
}