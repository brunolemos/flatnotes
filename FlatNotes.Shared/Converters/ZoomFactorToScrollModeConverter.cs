using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class ZoomFactorToScrollModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object useEnabled, string language)
        {
            double zoomFactor;
            double.TryParse(value.ToString(), out zoomFactor);

            bool _useEnabled = false;
            if (useEnabled != null && useEnabled.ToString() == Boolean.TrueString) _useEnabled = true;
            return zoomFactor > 1 ? (_useEnabled ? ScrollMode.Enabled : ScrollMode.Auto) : ScrollMode.Disabled;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            return 1;
        }
    }
}
