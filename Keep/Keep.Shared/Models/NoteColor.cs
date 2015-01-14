using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Keep.Models
{
    public class NoteColors : List<NoteColor> { }

    [DataContract]
    public sealed class NoteColor
    {
        [DataMember]
        public string Key { get; internal set; }
        public SolidColorBrush Color { get; internal set; }
        public SolidColorBrush DarkColor1 { get; private set; }
        public SolidColorBrush DarkColor2 { get; private set; }

        public static readonly NoteColor DEFAULT    = new NoteColor("DEFAULT",  Windows.UI.Color.FromArgb(0xFF, 0xF5, 0xF5, 0xF5));
        public static readonly NoteColor RED        = new NoteColor("RED",      Windows.UI.Color.FromArgb(0xFF, 0xFF, 0x6D, 0x3F));
        public static readonly NoteColor ORANGE     = new NoteColor("ORANGE",   Windows.UI.Color.FromArgb(0xFF, 0xFF, 0x97, 0x00));
        public static readonly NoteColor YELLOW     = new NoteColor("YELLOW",   Windows.UI.Color.FromArgb(0xFF, 0xFF, 0xE9, 0x00));
        public static readonly NoteColor GRAY       = new NoteColor("GRAY",     Windows.UI.Color.FromArgb(0xFF, 0xB8, 0xC4, 0xC9));
        public static readonly NoteColor BLUE       = new NoteColor("BLUE",     Windows.UI.Color.FromArgb(0xFF, 0x3F, 0xC3, 0xFF));
        public static readonly NoteColor TEAL       = new NoteColor("TEAL",     Windows.UI.Color.FromArgb(0xFF, 0x1C, 0xE8, 0xB5));
        public static readonly NoteColor GREEN      = new NoteColor("GREEN",    Windows.UI.Color.FromArgb(0xFF, 0x8A, 0xC2, 0x49));

        public static readonly Dictionary<string, NoteColor> Colors = new Dictionary<string, NoteColor>()
        {
            { DEFAULT.Key, DEFAULT},
            { RED.Key, RED},
            { ORANGE.Key, ORANGE},
            { YELLOW.Key, YELLOW},
            { GRAY.Key, GRAY},
            { BLUE.Key, BLUE},
            { TEAL.Key, TEAL},
            { GREEN.Key, GREEN},
        };

        public NoteColor()
        {
            this.Key = NoteColor.DEFAULT.Key;
            this.Color = NoteColor.DEFAULT.Color;
        }

        public NoteColor( string key )
        {
            if (!Colors.ContainsKey(key)) key = "DEFAULT";

            this.Key = Colors[key].Key;
            this.Color = Colors[key].Color;
        }

        private NoteColor(string key, Color color)
        {
            this.Key = key;
            this.Color = new SolidColorBrush(color);
        }

        public override string ToString()
        {
            return this.Color.Color.ToString();
        }
    }
}
