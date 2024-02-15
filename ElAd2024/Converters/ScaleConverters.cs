using ElAd2024.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ElAd2024.Converters;
public class IntToGramsStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is int) ? $"{value}g" : "N/A";

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BoolToGreenRedColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    => value is true ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}