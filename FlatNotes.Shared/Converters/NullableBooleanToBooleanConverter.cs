using System;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class NullableBooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
            bool boolValue = (value as bool?) == true;
            return invert != null && invert.ToString() == Boolean.TrueString ? !boolValue : boolValue;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            bool boolValue = (value as bool?) == true;
            return invert != null && invert.ToString() == Boolean.TrueString ? !boolValue : boolValue;
        }
    }
}
