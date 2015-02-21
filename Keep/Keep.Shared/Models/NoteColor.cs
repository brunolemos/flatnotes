using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Keep.Models
{
    public class NoteColors : List<NoteColor> { }

    [DataContract]
    public sealed class NoteColor
    {
        [DataMember]
        public string Key { get; internal set; }
        public string Color { get; internal set; }
        public string DarkColor1 { get; internal set; }
        public string DarkColor2 { get; internal set; }
        
        public static readonly NoteColor DEFAULT    = new NoteColor("DEFAULT",  "#f0f0f0", "#aaaaaa", "#999999");//f5f5f5
        public static readonly NoteColor RED        = new NoteColor("RED",      "#ff6d3f", "#ff6d3f", "#ea5f39");
        public static readonly NoteColor ORANGE     = new NoteColor("ORANGE",   "#ff9700", "#ff9700", "#f47b00");
        public static readonly NoteColor YELLOW     = new NoteColor("YELLOW",   "#ffe900", "#ffc000", "#f4a800");
        public static readonly NoteColor GRAY       = new NoteColor("GRAY",     "#b8c4c9", "#9badb6", "#8fa3ad");
        public static readonly NoteColor BLUE       = new NoteColor("BLUE",     "#3fc3ff", "#3fc3ff", "#00afff");
        public static readonly NoteColor TEAL       = new NoteColor("TEAL",     "#1ce8b5", "#1ce8b5", "#11c19f");
        public static readonly NoteColor GREEN      = new NoteColor("GREEN",    "#8ac249", "#8ac249", "#679e37");

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
            this.DarkColor1 = NoteColor.DEFAULT.DarkColor1;
            this.DarkColor2 = NoteColor.DEFAULT.DarkColor2;
        }

        public NoteColor( string key )
        {
            if (!Colors.ContainsKey(key)) key = "DEFAULT";

            this.Key = Colors[key].Key;
            this.Color = Colors[key].Color;
            this.DarkColor1 = Colors[key].DarkColor1;
            this.DarkColor2 = Colors[key].DarkColor2;
        }

        private NoteColor(string key, string color)
        {
            this.Key = key;
            this.Color = color;
            this.DarkColor1 = color;
            this.DarkColor2 = color;
        }

        private NoteColor(string key, string color, string darkColor1, string darkColor2)
        {
            this.Key = key;
            this.Color = color;
            this.DarkColor1 = String.IsNullOrEmpty(darkColor1) ? Color : darkColor1;
            this.DarkColor2 = String.IsNullOrEmpty(darkColor2) ? DarkColor1 : darkColor2;
        }

        public override string ToString()
        {
            return this.Color.ToString();
        }
    }
}
