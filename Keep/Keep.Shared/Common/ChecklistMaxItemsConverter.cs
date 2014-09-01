using System;
using System.Linq;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

using Keep.Models;

namespace Keep.Common
{
    public sealed class ChecklistMaxItemsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //Debug.WriteLine("ChecklistMaxItemsConverter");

            try
            {
                var list = (Checklist)value;
                int maxItems = int.Parse(parameter.ToString());

                if (list == null || list.Count <= 0 || maxItems < 0) return value;

                if (list.Count <= maxItems)
                {
                    return list;
                }
                else
                {
                    var result = list.Take<ChecklistItem>(Math.Max(1, maxItems - 1)); //(int)Math.Round(maxItems * 0.6)
                    return result;
                }
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
