using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class IsEqualToVisibilityConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ValueToCompareProperty = DependencyProperty.Register("ValueToCompare", typeof(object), typeof(IsEqualToVisibilityConverter), new PropertyMetadata(null));
        public object ValueToCompare { get { return (object)GetValue(ValueToCompareProperty); } set { SetValue(ValueToCompareProperty, value); } }

        public object Convert(object obj1, Type targetType, object invert, string language)
        {
            bool isEqual = obj1?.Equals(ValueToCompare) == true;
            bool isVisible = invert != null && invert.ToString() == Boolean.TrueString ? !isEqual : isEqual;

            //Debug.WriteLine("Comparing obj1:{0} obj2:{1} isEqual:{2}", obj1?.ToString(), ValueToCompare?.ToString(), isEqual);

            return isEqual ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            return null;
        }
    }
}
