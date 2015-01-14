using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Keep.Converters
{
    public sealed class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value == null || String.IsNullOrEmpty(value.ToString())) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
