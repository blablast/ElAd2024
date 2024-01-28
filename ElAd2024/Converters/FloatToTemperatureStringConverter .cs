using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.UI.Xaml.Data;

namespace ElAd2024.Converters;
public class FloatToTemperatureStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is float temperature)
        {
            return $"{temperature:0.0} °C";
        }
        return "N/A"; // Or any default value you prefer
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
