using System;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class IsMobileToGridRowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
            bool isMobile = value is bool && (bool)value;
            if (invert != null && invert.ToString() == Boolean.TrueString) isMobile = !isMobile;
            
            return isMobile ? 2 : 0;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            bool isMobile = value.ToString() == "0";
            if (invert != null && invert.ToString() == Boolean.TrueString) isMobile = !isMobile;

            return isMobile;
        }
    }
}
