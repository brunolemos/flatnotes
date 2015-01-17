using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Keep.Converters
{
    public sealed class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isNull = (value == null || String.IsNullOrEmpty(value.ToString()));

            if (value is DateTime) isNull |= ((DateTime)value).CompareTo(DateTime.MinValue) == 0;
            
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
