using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;

namespace ElAd2024.Converters;

public class IsMinusPolarityToBrushConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? new SolidColorBrush(Colors.DeepSkyBlue) : new SolidColorBrush(Colors.Red)) : new SolidColorBrush(Colors.Gray);
}

public class IsPlusPolarityToBrushConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.DeepSkyBlue)) : new SolidColorBrush(Colors.Gray);
}


public class IsMinusPolarityToStringConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? "-" : "+") : "N/A";
}

public class IsPlusPolarityToStringConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue ) ? (targetValue ? "+" : "-") : "N/A";
}


public class DateTimeToStringConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is DateTime dateTime && parameter is string format) ? dateTime.ToString(format) : string.Empty;
}
public class BoolToNotVisibilityConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? Visibility.Collapsed : Visibility.Visible) : Visibility.Collapsed;
}
public  class BoolToVisibilityConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
}

public class BoolToOpacityConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? 0.65 : 1) : 1;

}

public abstract class AbstractConverter : IValueConverter
{
    public virtual object Convert(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

    public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}