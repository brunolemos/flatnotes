using System;
using System.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

using Keep.Models;
using Keep.Common;

namespace Keep.Converters
{
    public sealed class ChecklistMaxItemsConverter : Notifiable, IValueConverter
    {
        public int MaxItems { get { return maxItems; } set { maxItems = value; } }
        private int maxItems = 6;

        public bool IsTrimmed { get { return isTrimmed; } set { isTrimmed = value; NotifyPropertyChanged("IsTrimmed"); } }
        private bool isTrimmed = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IsTrimmed = false;

            try
            {
                var list = (Checklist)value;

                if (list == null || list.Count <= 0 || MaxItems < 0) return value;
                if (list.Count <= MaxItems) return list;

                IsTrimmed = true;
                var result = list.Take<ChecklistItem>(Math.Max(1, MaxItems - 1)); //(int)Math.Round(maxItems * 0.6)

                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message);
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
