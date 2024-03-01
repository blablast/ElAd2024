using CommunityToolkit.Mvvm.ComponentModel;
using ElAd2024.Models.Database;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using ElAd2024.Helpers;

namespace ElAd2024.ViewModels;

public partial class AlgorithmStepViewModel : ObservableObject
{
    public AlgorithmStep AlgorithmStep { get; private set; }
    public int Id { get; }
    [ObservableProperty] private int order;
    [ObservableProperty] private string frontName = string.Empty;
    [ObservableProperty] private string backName = string.Empty;
    [ObservableProperty] private string? actionParameter = null;

    partial void OnBackNameChanged(string value)
    {
        IconBack = string.Empty;
    }

    [ObservableProperty] private bool isFrozen = true;
    [ObservableProperty] private double opacity;
    [ObservableProperty] private TimeSpan updateInterval;
    [ObservableProperty] private Brush background;
    [ObservableProperty] private Brush foreground;
    [ObservableProperty] private string iconFront;
    [ObservableProperty] private string iconBack = string.Empty;
    [ObservableProperty] private string imageSource = " ";
    public bool IsFirst => AlgorithmStep?.Step.IsLast ?? false;
    public bool IsLast => AlgorithmStep?.Step.IsLast ?? false;
    public bool IsMoveable => AlgorithmStep?.Step.IsMoveable ?? false;

    public AlgorithmStepViewModel(AlgorithmStep algorithmStep)
    {
        AlgorithmStep = algorithmStep;
        Id = algorithmStep.Id;
        IconFront = AlgorithmStep.Step.Icon;
        Background = new SolidColorBrush(Color.FromArgb(100, AlgorithmStep.Step.Background.Color.R, AlgorithmStep.Step.Background.Color.G, AlgorithmStep.Step.Background.Color.B));
        Foreground = AlgorithmStep.Step.Foreground;
        FrontName = AlgorithmStep.FrontName.Replace("\\n", "\n");
        BackName = AlgorithmStep.BackName.Replace("\\n", "\n");
        ActionParameter = AlgorithmStep.ActionParameter;
        Order = AlgorithmStep.Order;
    }

    public bool IsCorrectParameter(string parameter)
    {
        if (AlgorithmStep.Step.HasParameter && ActionParameter is not null)
        {
            return (AlgorithmStep.Step.IsNumericParameter) ? int.TryParse(parameter, out _) : FileNameCheckerHelper.IsValidFileName(ActionParameter);
        }
        return !(AlgorithmStep.Step.HasParameter);
    }

    public void FlipSlow()
        => UpdateInterval = TimeSpan.FromMilliseconds((new Random()).Next(1500, 3000));
    public void Reset()
    {
        IconBack = IconFront;
        IsFrozen = true;
        Opacity = 0.2;
        ImageSource = " ";
        BackName = AlgorithmStep?.BackName.Replace("\\n", "\n") ?? string.Empty;
        UpdateInterval = TimeSpan.FromMilliseconds(900);
    }
}
