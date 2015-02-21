using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Keep.Converters
{
    public sealed class ReorderModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
#if WINDOWS_PHONE_APP
            bool result = (value is ListViewReorderMode && (ListViewReorderMode)value == ListViewReorderMode.Enabled) ? true : false;
#else
            bool result = false;
#endif

            if (invert != null && invert.ToString() == Boolean.TrueString) result = !result;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
#if WINDOWS_PHONE_APP
            var boolValue = value is bool && (bool)value;
            if (invert != null && invert.ToString() == Boolean.TrueString) boolValue = !boolValue;

            return boolValue ? ListViewReorderMode.Enabled : ListViewReorderMode.Disabled;
#else
            return null;
#endif
        }
    }
}
