using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Keep.Converters
{
    public sealed class ReorderModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
#if WINDOWS_PHONE_APP
            bool boolValue = (value is ListViewReorderMode && (ListViewReorderMode)value == ListViewReorderMode.Enabled) ? true : false;
#else
            bool boolValue = false;
#endif

            if (invert != null && invert.ToString() == Boolean.TrueString) boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
#if WINDOWS_PHONE_APP
            var boolValue = value is Visibility && (Visibility)value == Visibility.Visible;
            if (invert != null && invert.ToString() == Boolean.TrueString) boolValue = !boolValue;

            return boolValue ? ListViewReorderMode.Enabled : ListViewReorderMode.Disabled;
#else
            return null;
#endif
        }
    }
}
