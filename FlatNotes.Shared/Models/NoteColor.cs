using FlatNotes.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace FlatNotes.Models
{
    public class NoteColors : List<NoteColor>
    {
        public static implicit operator NoteColors(FlatNotes.Utils.Migration.Versions.v2.Models.NoteColors _noteColors)
        {
            var noteColors = new NoteColors();
            foreach (var item in _noteColors)
                noteColors.Add(item);

            return noteColors;
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.NoteColors(NoteColors _noteColors)
        {
            var noteColors = new FlatNotes.Utils.Migration.Versions.v2.Models.NoteColors();
            foreach (var item in _noteColors)
                noteColors.Add(item);

            return noteColors;
        }
    }

    [DataContract]
    public sealed class NoteColor
    {
        [DataMember]
        public string Key { get; internal set; }
        public SolidColorBrush Color { get; internal set; }
        public SolidColorBrush DarkColor { get; internal set; }

        public static readonly NoteColor DEFAULT    = new NoteColor("DEFAULT",  "#FAFAFA", "#AAAAAA");
        public static readonly NoteColor RED        = new NoteColor("RED",      "#FF8A80", "#FF6D3F");
        public static readonly NoteColor ORANGE     = new NoteColor("ORANGE",   "#FFD180", "#FF9700");
        public static readonly NoteColor YELLOW     = new NoteColor("YELLOW",   "#FFFF8D", "#FFC000");
        public static readonly NoteColor GRAY       = new NoteColor("GRAY",     "#CFD8DC", "#9BADB6");
        public static readonly NoteColor BLUE       = new NoteColor("BLUE",     "#80D8FF", "#3FC3ff");
        public static readonly NoteColor TEAL       = new NoteColor("TEAL",     "#A7FFEB", "#1CE8b5");
        public static readonly NoteColor GREEN      = new NoteColor("GREEN",    "#CCFF90", "#8AC249");

        private static readonly Dictionary<string, NoteColor> colorsDictionary = new Dictionary<string, NoteColor>()
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

        public static NoteColors Colors = new NoteColors()
        {
            NoteColor.DEFAULT,
            NoteColor.RED,
            NoteColor.ORANGE,
            NoteColor.YELLOW,
            NoteColor.GRAY,
            NoteColor.BLUE,
            NoteColor.TEAL,
            NoteColor.GREEN
        };

        public NoteColor()
        {
            this.Key = NoteColor.DEFAULT.Key;
            this.Color = NoteColor.DEFAULT.Color;
        }

        public NoteColor( string key )
        {
            if (string.IsNullOrEmpty(key) || !colorsDictionary.ContainsKey(key)) key = "DEFAULT";

            this.Key = colorsDictionary[key].Key;
            this.Color = colorsDictionary[key].Color;
            this.DarkColor = colorsDictionary[key].DarkColor;
        }

        private NoteColor(string key, string color)
        {
            this.Key = key;
            this.Color = new SolidColorBrush(new Color().FromHex(color));
            this.DarkColor = this.Color;
        }

        private NoteColor(string key, string color, string darkColor)
        {
            this.Key = key;
            this.Color = new SolidColorBrush(new Color().FromHex(color));
            this.DarkColor = new SolidColorBrush(new Color().FromHex(darkColor));
        }

        public override string ToString()
        {
            return this.Color.Color.ToString();
        }

        public static implicit operator NoteColor(FlatNotes.Utils.Migration.Versions.v2.Models.NoteColor noteColor)
        {
            return new NoteColor(noteColor.Key);
        }

        public static implicit operator FlatNotes.Utils.Migration.Versions.v2.Models.NoteColor(NoteColor noteColor)
        {
            return new FlatNotes.Utils.Migration.Versions.v2.Models.NoteColor(noteColor.Key);
        }

        public static implicit operator SolidColorBrush(NoteColor noteColor)
        {
            return noteColor.Color;
        }

        public static implicit operator Color(NoteColor noteColor)
        {
            return noteColor.Color.Color;
        }
    }
}
