using ElAd2024.Helpers;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ElAd2024.Converters;

public class FloatToHumidityStringConverter : BaseEnviromentalToStringConverter
{
    public FloatToHumidityStringConverter()
    {
        suffix = "%";
    }
}

public class HumidityToBrushConverter : BaseEnviromentalToStringConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
        => (value is float targetValue)
           ? ColorInterpolation.GetBrushBasedOnHumidity(targetValue / 100, Colors.Yellow, Colors.Green, Colors.Blue)
           : new SolidColorBrush(Colors.Gray); // Use a default color that makes sense for your application
}

public class FloatToTemperatureStringConverter : BaseEnviromentalToStringConverter
{
    public FloatToTemperatureStringConverter()
    {
        suffix = " °C";
    }
}

public class TemperatureToBrushConverter : BaseEnviromentalToStringConverter
{
    public override object Convert(object value, Type targetType, object parameter, string language)
    => (value is float targetValue)
       ? ColorInterpolation.GetBrushBasedOnHumidity(targetValue / 50, Colors.Blue, Colors.Green, Colors.Red)
       : new SolidColorBrush(Colors.Gray); // Use a default color that makes sense for your application
}

public abstract class BaseEnviromentalToStringConverter : IValueConverter
{
    protected string suffix = string.Empty;
    public virtual object Convert(object value, Type targetType, object parameter, string language)
        => $"{value:0.0}{suffix}"; // Or any default value you prefer

    public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}