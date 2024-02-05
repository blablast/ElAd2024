using Microsoft.UI.Xaml.Data;
using Windows.Devices.SerialCommunication;

namespace ElAd2024.Converters;
public class StringToSerialHandshakeConverter : EnumToStringConverter<SerialHandshake> { }
public class StringToSerialParityConverter : EnumToStringConverter<SerialParity> { }
public class StringToSerialStopBitCountConverter : EnumToStringConverter<SerialStopBitCount> { }


public class EnumToStringConverter<TEnum> : IValueConverter where TEnum : struct
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => (value is TEnum enumValue) ? enumValue.ToString() ?? string.Empty : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => (value is string stringValue && Enum.TryParse<TEnum>(stringValue, out var enumValue)) ? enumValue : default;
}