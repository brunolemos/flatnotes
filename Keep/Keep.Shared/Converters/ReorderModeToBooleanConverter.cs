using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Keep.Converters
{
    public sealed class ReorderModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
            bool result = (value is ListViewReorderMode && (ListViewReorderMode)value == ListViewReorderMode.Enabled) ? true : false;
            if (invert != null && invert.ToString() == Boolean.TrueString) result = !result;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            var boolValue = value is bool && (bool)value;
            if (invert != null && invert.ToString() == Boolean.TrueString) boolValue = !boolValue;

            return boolValue ? ListViewReorderMode.Enabled : ListViewReorderMode.Disabled;
        }
    }
}
