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

                if (list.Count >= 1)
                {
                    var selected = list.FirstOrDefault((i) => i.IsSelected);
                    var selectedIndex = list.IndexOf(selected);
                    return selectedIndex >= 0 ? list[selectedIndex] : list[0];
                }
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
