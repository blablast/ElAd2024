using ElAd2024.Services;

namespace ElAd2024.Contracts.Services;
public interface IRobotService
{
    string IPAddress { get; set; }
    bool IsConnected { get; }
    List<string> FanucVisions { get; set; }

    public PositionXYZWPR CurrentPosition { get; }

    /// <summary>Gets fanuc visions.</summary>
    void GetFanucVisions();

    /// <summary>Gets the flag status.</summary>
    bool GetFlagRegister(int index);
    (int?, double?) GetNumericRegister(int index);
    string GetStringRegister(int index);

    bool SetRegister(int index, bool value);
    bool SetRegister(int index, int value);
    bool SetRegister(int index, double value);
    bool SetRegister(int index, string value);

    Task<bool> IsConnectedAsync();
    Task<bool> ConnectAsync();
    void ChangeOverride(int value);
    string RobotVistionURL();

}