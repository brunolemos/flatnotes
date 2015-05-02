using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
            bool boolValue = value is bool && (bool)value;
            if (invert != null && invert.ToString() == Boolean.TrueString) boolValue = !boolValue;
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            bool boolValue = value is Visibility && (Visibility)value == Visibility.Visible;
            if (invert != null && invert.ToString() == Boolean.TrueString) boolValue = !boolValue;

            return boolValue;
        }
    }
}
