using ElAd2024.Devices;
using ElAd2024.Services;

namespace ElAd2024.Contracts.Devices;
public interface IRobotDevice : IDevice
{
    string IpAddress
    {
        get; set;
    }
    List<string> RobotVisions
    {
        get; set;
    }

    public PositionXyzWpr CurrentPosition
    {
        get;
    }

    void GetRobotVisions();

    Task<bool> GetFlagRegisterAsync(int index);
    Task<(int?, double?)> GetNumericRegisterAsync(int index);
    Task<string> GetStringRegisterAsync(int index);

    Task<bool> SetRegisterAsync(int index, bool value);
    Task<bool> SetRegisterAsync(int index, int value);
    Task<bool> SetRegisterAsync(int index, double value);
    Task<bool> SetRegisterAsync(int index, string value);

    void ChangeOverride(int value);
    string RobotVisionUrl();

}