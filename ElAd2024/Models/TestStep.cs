using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Devices.Bluetooth.Advertisement;

namespace ElAd2024.Models;

public partial class TestStep : ObservableRecipient
{
    public enum StepType
    {
        Computer,
        Robot,
        Pad,
        Enviroment,
        Scale
    }
    private static (SolidColorBrush, SolidColorBrush, string) computer = ( new(Colors.Orange), new(Colors.Black), "\uEC4E");
    private static (SolidColorBrush, SolidColorBrush, string) robot = (new(Colors.Lime), new(Colors.Black), "\uE99A");
    private static (SolidColorBrush, SolidColorBrush, string) pad = (new(Colors.DarkRed), new(Colors.White), "\uE75E");
    private static (SolidColorBrush, SolidColorBrush, string) enviroment = (new(Colors.Violet), new(Colors.Black), "\uE957");
    private static (SolidColorBrush, SolidColorBrush, string) scale = (new(Colors.Beige), new(Colors.Black), "\uE8FE");

    public int Order { get; set; }
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string backContent = string.Empty;
    partial void OnBackContentChanged(string value)
    {
        IconBack = string.Empty;
    }
    [ObservableProperty] private bool isFrozen = true;
    [ObservableProperty] private double opacity = 0.5;
    [ObservableProperty] private TimeSpan updateInterval = TimeSpan.FromMicroseconds(900);
    public Brush Foreground { get; } = new SolidColorBrush(Colors.Red);
    public Brush Background { get; } = new SolidColorBrush(Colors.Transparent);
    public string Icon { get; } = "\uE712";
    public string IconBack { get; private set; } = "\uE712";
    [ObservableProperty] private string imageSource = " ";

    public TestStep(string title, string backContent, StepType type, int order)
    {
        Title = title;
        BackContent = backContent;
        Order = order;     
        switch (type)
        {
            case StepType.Computer:
                (Background, Foreground, Icon) = computer;
                break;
            case StepType.Robot:
                (Background, Foreground, Icon) = robot;
                break;
            case StepType.Pad:
                (Background, Foreground, Icon) = pad;
                break;
            case StepType.Enviroment:
                (Background, Foreground, Icon) = enviroment;
                break;
            case StepType.Scale:
                (Background, Foreground, Icon) = scale;
                break;
        }
    }

    public void FlipSlow()
    {
        Random random = new();
        UpdateInterval = TimeSpan.FromMilliseconds(random.Next(1500, 3000));
    }
    public void Reset()
    {
        IconBack = Icon;
        IsFrozen = true;
        Opacity = 0.5;
        ImageSource = " ";
        BackContent = Title;
        UpdateInterval = TimeSpan.FromMilliseconds(900);
    }

}
