using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI;

using Keep.Models;

namespace Keep.Utils
{
    public static class Extensions
    {
        public static long ToTimestamp( this DateTime dateTime ) {
            return (long)(dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static string RemoveNewLines(this string str)
        {
            return str.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ").Trim();
        }

        public static Color FromHex( this Color color, String hex )
        {
            //remove the # at the front
            hex = hex.Replace( "#", "" );

            byte a = 255;
            byte r = 255;
            byte g = 255;
            byte b = 255;

            int start = 0;

            //handle ARGB strings (8 characters long)
            if ( hex.Length == 8 )
            {
                a = byte.Parse( hex.Substring( 0, 2 ), System.Globalization.NumberStyles.HexNumber );
                start = 2;
            }

            //convert RGB characters to bytes
            r = byte.Parse( hex.Substring( start, 2 ), System.Globalization.NumberStyles.HexNumber );
            g = byte.Parse( hex.Substring( start + 2, 2 ), System.Globalization.NumberStyles.HexNumber );
            b = byte.Parse( hex.Substring( start + 4, 2 ), System.Globalization.NumberStyles.HexNumber );

            return Color.FromArgb( a, r, g, b );
        }

        public static Notes Merge(this Notes notes, Notes list)
        {
            if (notes == null) notes = new Notes();
            if (list == null || list.Count <= 0) return notes;
            if (notes.Count <= 0) return list;

            foreach (Note item in list)
            {
                Note existentItem = notes.Where<Note>(x => x.GetID() == item.GetID()).FirstOrDefault();

                if (existentItem != null && !String.IsNullOrEmpty(existentItem.GetID()))
                {
                    //merge - replace if newer or ignore if older
                    if (item.GetUpdatedAt().CompareTo(existentItem.GetUpdatedAt()) >= 0)
                    {
                        int pos = notes.IndexOf(existentItem);
                        notes[pos] = item;
                    }
                }
                else
                {
                    notes.Add(item);
                }
            }

            return notes;
        }
    }
}
