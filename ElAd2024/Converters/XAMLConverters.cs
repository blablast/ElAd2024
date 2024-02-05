using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace ElAd2024.Converters
{
    public  class BoolToVisibilityConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
            => (value is bool targetValue) ? Visibility.Visible : Visibility.Collapsed;

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }

}
