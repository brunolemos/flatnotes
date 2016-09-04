using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class ZoomFactorToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object useVisible, string language)
        {
            double zoomFactor;
            double.TryParse(value.ToString(), out zoomFactor);

            bool _useVisible = false;
            if (useVisible != null && useVisible.ToString() == Boolean.TrueString) _useVisible = true;
            return zoomFactor > 1 ? (_useVisible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Auto) : ScrollBarVisibility.Disabled;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            return 1;
        }
    }
}
