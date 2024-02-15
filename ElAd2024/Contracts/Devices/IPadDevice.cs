using System.Collections.ObjectModel;
using System.ComponentModel;
using ElAd2024.Models.Database;

namespace ElAd2024.Contracts.Devices;

public interface IPadDevice : IDevice, INotifyPropertyChanged
{
    int AxisMinVoltage { get; }
    int AxisMaxVoltage { get; }
    ObservableCollection<Voltage> Voltages { get; }
    int? Value { get; }
    byte Phase { get; }
    int Elapsed { get; }
    Task Setup(object parameters);
    Task StartCycle(bool isPlusPolarity);
    Task StopCycle(bool trimData);
}