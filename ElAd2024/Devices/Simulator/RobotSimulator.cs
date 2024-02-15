using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Contracts.Devices;
using ElAd2024.Devices.Simulator;

namespace ElAd2024.Devices;

public partial class RobotSimulator : BaseSimulator, IRobotDevice
{
    #region Properties
    public string IpAddress { get; set; } = string.Empty;
    public List<string> RobotVisions { get; set; } = ["Item 1", "Item 2", "Item 3", "Item 4"];
    public string RobotVisionUrl() => string.Empty;
    #endregion

    #region Public Methods

    public void GetRobotVisions()
    {
    } // RobotVisions = ["Item 1", "Item 2", "Item 3", "Item 4"];

    public async Task<bool> SetRegisterAsync(int index, bool value)
    {
        Debug.WriteLine($"Simulating set bool register {index} to value {value}");
        await Task.CompletedTask;
        return true;
    }
    public async Task<bool> SetRegisterAsync(int index, int value)
    {
        Debug.WriteLine($"Simulating set int register {index} to value {value}");
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> SetRegisterAsync(int index, double value)
    {
        Debug.WriteLine($"Simulating set double register {index} to value {value}");
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> SetRegisterAsync(int index, string value)
    {
        Debug.WriteLine($"Simulating set string register {index} to value {value}");
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> GetFlagRegisterAsync(int index)
    {
        IsIndexInRange(index, 1, 200);
        await Task.Delay(2000);
        return true;
    }

    public async Task<(int?, double?)> GetNumericRegisterAsync(int index)
    {
        IsIndexInRange(index, 1, 200);
        await Task.CompletedTask;
        return (null, 1.0);
    }

    public async Task<string> GetStringRegisterAsync(int index)
    {
        IsIndexInRange(index, 1, 25);
        await Task.CompletedTask;
        return "Simulated string";
    }

    public void ChangeOverride(int value)
    {
        Debug.WriteLine($"Simulating change override to {value}");
    }

    public PositionXyzWpr CurrentPosition => new() { X = 1.0, Y = 2.0, Z = 3.0, W = 4.0, P = 5.0, R = 6.0 };

    #endregion

    #region private Methods

    private static bool IsIndexInRange(int index, int min, int max) => index < min || index > max
        ? throw new ArgumentOutOfRangeException(nameof(index))
        : true;

    #endregion
}
