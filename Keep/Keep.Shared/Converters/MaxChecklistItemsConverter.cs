using System;
using System.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

using Keep.Models;
using Keep.Common;

namespace Keep.Converters
{
    public sealed class MaxChecklistItemsConverter : Notifiable, IValueConverter
    {
        public int MaxItems { get { return maxItems; } set { maxItems = value; } }
        private int maxItems = 6;

        public bool IsTrimmed { get { return isTrimmed; } set { isTrimmed = value; NotifyPropertyChanged("IsTrimmed"); } }
        private bool isTrimmed = false;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            IsTrimmed = false;

            int intParameter;
            if (parameter != null && int.TryParse(parameter.ToString(), out intParameter)) MaxItems = intParameter;

            try
            {
                var list = value as Checklist;

                if (list == null || MaxItems <= 0 || list.Count <= 1) return value;

                var orderedList = list.OrderBy<ChecklistItem, bool>(item => item.IsChecked).ToList<ChecklistItem>();
                if (orderedList.Count <= MaxItems) return orderedList;

                IsTrimmed = true;
                return orderedList.Take<ChecklistItem>(Math.Max(1, MaxItems));
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
