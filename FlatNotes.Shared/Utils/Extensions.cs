using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace FlatNotes.Utils
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

        public static Color FromHex(this Color color, String hex)
        {
            return hexToColor(hex);
        }

        public static SolidColorBrush fromHex(this SolidColorBrush color, String hex)
        {
            return new SolidColorBrush(hexToColor(hex));
        }

        private static Color hexToColor(String hex)
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

        public static Color Add(this Color color_a, Color color_b)
        {
            float b_opacity_percent = (float)color_b.A / 0xff;
            float a_opacity_percent = 1 - b_opacity_percent;
            
            byte a = (byte)(0xff * Math.Min(a_opacity_percent + b_opacity_percent, 1));
            byte r = (byte)(color_a.R * a_opacity_percent + color_b.R * b_opacity_percent);
            byte g = (byte)(color_a.G * a_opacity_percent + color_b.G * b_opacity_percent);
            byte b = (byte)(color_a.B * a_opacity_percent + color_b.B * b_opacity_percent);

            return Color.FromArgb(a, r, g, b);
        }
    }
}
