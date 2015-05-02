using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class PositiveIntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
            bool isPositive = (value is int && (int)value > 0);
            if (invert != null && invert.ToString() == Boolean.TrueString) isPositive = !isPositive;

            return isPositive ? Visibility.Visible : Visibility.Collapsed;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
