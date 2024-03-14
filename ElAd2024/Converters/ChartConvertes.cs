using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ElAd2024.Converters;

public class PhaseToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is byte targetValue) ?
        targetValue switch
            {
                1 => new SolidColorBrush(Colors.Red),
                2 => new SolidColorBrush(Colors.Blue),
                3 => new SolidColorBrush(Colors.Red),
                4 => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.Gray)
        }
        : new SolidColorBrush(Colors.Gray);

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class HighVoltageToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
     => (value is int targetValue) ? $"{targetValue / 1000.0:0.00} kV" : "N/A";

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

}

public class ElapsedTimeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
     => (value is int targetValue) ? $"{targetValue / 10.0:0.00} s" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();

}
