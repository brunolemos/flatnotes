﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class NullableToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object invert, string language)
        {
            bool isNull = (value == null || String.IsNullOrEmpty(value.ToString()));
            if (value is DateTime) isNull |= ((DateTime)value).CompareTo(DateTime.MinValue) == 0;

            isNull = invert != null && invert.ToString() == Boolean.TrueString ? !isNull : isNull;

            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
