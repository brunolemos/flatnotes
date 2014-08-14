using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

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

        public static async Task<RenderTargetBitmap> ToBitmap(this FrameworkElement element)
        {
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element, width, height);
            return renderTargetBitmap;
            //IBuffer pixels = await renderTargetBitmap.GetPixelsAsync();
            //IRandomAccessStream stream = pixels.AsStream().AsRandomAccessStream();

            //var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
            //byte[] bytes = pixels.ToArray();
            //encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, bytes);
            //stream.Seek(0);

            ////await encoder.FlushAsync();
            //return stream;
        }
    }
}
