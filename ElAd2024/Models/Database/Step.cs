using System.ComponentModel.DataAnnotations.Schema;
using ElAd2024.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace ElAd2024.Models.Database;

public class Step
{
    public enum DeviceType
    {
        Computer,
        Robot,
        Pad,
        Environment,
        Scale
    }

    public int Id { get; set; }
    public string BackgroundColor { get; set; } = Colors.Gray.ToString();
    public string ForegroundColor { get; set; } = Colors.Black.ToString();
    public string Icon { get; set; } = "\uE712";
    public bool IsFirst { get; set; } = false;
    public bool IsLast { get; set; } = false;
    public bool IsMoveable { get; set; } = true;
    public bool IsMandatory { get; set; } = false;
    public bool HasParameter { get; set; } = false;
    public bool IsNumericParameter { get; set; } = false;
    public string AsyncActionName { get; set; } = string.Empty;

    public ICollection<AlgorithmStep> AlgorithmSteps { get; set; } = [];
    public ICollection<TestStep> TestSteps { get; set; } = [];

    [NotMapped]
    public SolidColorBrush Background
    {
        get => GetColor(BackgroundColor);
        set => BackgroundColor = value.Color.ToString();
    }

    [NotMapped]
    public SolidColorBrush Foreground
    {
        get => GetColor(ForegroundColor);
        set => ForegroundColor = value.Color.ToString();
    }

    [NotMapped]
    public DeviceType Style
    {
        set => (Background, Foreground, Icon) = GetDeviceTypeStyle(value);
    }

    private static (SolidColorBrush, SolidColorBrush, string) GetDeviceTypeStyle(DeviceType deviceType)
        => deviceType switch
        {
            DeviceType.Computer => (GetColor("#FF3282F6"), new(Colors.Black), "\uEC4E"),
            DeviceType.Robot => (new(Colors.Lime), new(Colors.Black), "\uE99A"),
            DeviceType.Pad => (GetColor("#FFFF5151"), new(Colors.White), "\uE75E"),
            DeviceType.Environment => (GetColor("#FF235911"), new(Colors.White), "\uE957"),
            DeviceType.Scale => (new(Colors.Beige), new(Colors.Black), "\uE8FE"),
            _ => throw new ArgumentOutOfRangeException(nameof(deviceType), $"Not expected device type value: {deviceType}"),
        };

    private static SolidColorBrush GetColor(string color, byte transparency = 255) => new SolidColorBrush(
               Color.FromArgb(
                              transparency,
                              byte.Parse(color[3..5], System.Globalization.NumberStyles.HexNumber),
                              byte.Parse(color[5..7], System.Globalization.NumberStyles.HexNumber),
                              byte.Parse(color[7..9], System.Globalization.NumberStyles.HexNumber)
                              )) ?? new SolidColorBrush(Colors.Gray);


}
