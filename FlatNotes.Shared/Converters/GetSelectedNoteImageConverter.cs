using FlatNotes.Models;
using System;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class GetSelectedNoteImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object limit, string language)
        {
            try
            {
                var list = (NoteImages)value;
                if (list != null && list.Count >= 1) return list.GetSelectedNoteImageOrLast();
            }
            catch (Exception) { }

            return new NoteImage();
        }

        public object ConvertBack(object value, Type targetType, object invert, string language)
        {
            return null;
        }
    }
}
