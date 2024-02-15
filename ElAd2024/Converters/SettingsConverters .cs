using System.Globalization;
using Microsoft.UI.Xaml.Data;
namespace ElAd2024.Converters;

public class StringToUintConverter : StringToNumericConverter<uint> { }
public class StringToUshortConverter : StringToNumericConverter<ushort> { }
public class StringToNumericConverter<TNumeric> : IValueConverter where TNumeric : IConvertible
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value?.ToString() ?? string.Empty;

    public object? ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not string stringValue)
        {
            return default;
        }

        try
        {
            return (TNumeric)System.Convert.ChangeType(stringValue, typeof(TNumeric), CultureInfo.InvariantCulture);
        }
        catch
        {
            // ignored
        }
        return default;
    }
}

public class DoubleToTimeSpanFromMillisecondsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is TimeSpan timeSpan) ? (double)timeSpan.TotalMilliseconds : 0;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => (value is double milliseconds) ? TimeSpan.FromMilliseconds(milliseconds) : TimeSpan.Zero;

}