using FlatNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class SliceNoteImagesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object limit, string language)
        {
            //try
            //{
                var list = (NoteImages)value;

                int _limit;
                int.TryParse(limit.ToString(), out _limit);

                if (_limit >= 1) return list.Take(_limit);
            //}
            //catch (Exception)
            //{
            //}

            return value;
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            return value;
        }
    }
}
