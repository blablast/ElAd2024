using System.Collections.ObjectModel;
using System.Globalization;
using ElAd2024.Contracts.Services;
using ElAd2024.Models;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ElAd2024.Converters;

public class TimeToStringConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is DateTime dateTime) ? dateTime.ToString("HH:mm:ss") : "N/A";
}
public class ImageToFullPathConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => Path.Combine(App.GetService<ILocalSettingsService>().PicturesFolder, value?.ToString() ?? string.Empty);
}

public class IsSimulatedToBrushConverter : AbstractConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => new SolidColorBrush(
            (value is bool targetValue && targetValue) 
            ? new Windows.UI.Color() { R = 255, G = 68, B = 0, A = 44 } 
            : new Windows.UI.Color() { R = 0, G = 0, B = 0, A = 0 });
}
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
        => (value is bool targetValue) ? (targetValue ? "+" : "-") : "N/A";
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
public class BoolToVisibilityConverter : AbstractConverter
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

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string enumString)
        {
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
        }

        if (!Enum.IsDefined(typeof(ElementTheme), value))
        {
            throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
        }
        return Enum.Parse(typeof(ElementTheme), enumString).Equals(value);

    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(typeof(ElementTheme), enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}