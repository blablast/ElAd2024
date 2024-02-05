using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ElAd2024.Converters;

public  class BoolToVisibilityConverter : IValueConverter
{
    public virtual object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;

    public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public class BoolToOpacityConverter : IValueConverter
{
    public virtual object Convert(object value, Type targetType, object parameter, string language)
        => (value is bool targetValue) ? (targetValue ? 0.65 : 1) : 1;

    public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
