using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FlatNotes.Utils.Migration.Versions.v1.Models
{
    public class NoteColors : List<NoteColor> { }

    [DataContract]
    public sealed class NoteColor
    {
        [DataMember]
        public string Key { get; private set; }
        public string Color { get; private set; }
        public string DarkColor1 { get; private set; }
        public string DarkColor2 { get; private set; }

        public static readonly NoteColor DEFAULT    = new NoteColor("DEFAULT",  "#f5f5f5", "#aaaaaa", "#999999");// ((SolidColorBrush)App.Current.Resources["AppNoteDefaultBrush"]).Color.ToString());
        public static readonly NoteColor RED        = new NoteColor("RED",      "#ff6d3f", "#ff6d3f", "#ea5f39");// ((SolidColorBrush)App.Current.Resources["AppNoteRedBrush"]).Color.ToString());
        public static readonly NoteColor ORANGE     = new NoteColor("ORANGE",   "#ff9700", "#ff9700", "#f47b00");// ((SolidColorBrush)App.Current.Resources["AppNoteOrangeBrush"]).Color.ToString());
        public static readonly NoteColor YELLOW     = new NoteColor("YELLOW",   "#ffe900", "#ffc000", "#f4a800");// ((SolidColorBrush)App.Current.Resources["AppNoteYellowBrush"]).Color.ToString());
        public static readonly NoteColor GRAY       = new NoteColor("GRAY",     "#b8c4c9", "#9badb6", "#8fa3ad");// ((SolidColorBrush)App.Current.Resources["AppNoteGrayBrush"]).Color.ToString());
        public static readonly NoteColor BLUE       = new NoteColor("BLUE",     "#3fc3ff", "#3fc3ff", "#00afff");// ((SolidColorBrush)App.Current.Resources["AppNoteBlueBrush"]).Color.ToString());
        public static readonly NoteColor TEAL       = new NoteColor("TEAL",     "#1ce8b5", "#1ce8b5", "#11c19f");// ((SolidColorBrush)App.Current.Resources["AppNoteTealBrush"]).Color.ToString());
        public static readonly NoteColor GREEN      = new NoteColor("GREEN",    "#8ac249", "#8ac249", "#679e37");// ((SolidColorBrush)App.Current.Resources["AppNoteGreenBrush"]).Color.ToString());

        private static readonly List<NoteColor> ColorsList = new List<NoteColor>()
        {
            { NoteColor.DEFAULT },
            { NoteColor.RED },
            { NoteColor.ORANGE },
            { NoteColor.YELLOW },
            { NoteColor.GRAY },
            { NoteColor.BLUE },
            { NoteColor.TEAL },
            { NoteColor.GREEN },
        };

        private static readonly Dictionary<string, NoteColor> Colors = new Dictionary<string, NoteColor>()
        {
            { ColorsList[0].Key, ColorsList[0]},
            { ColorsList[1].Key, ColorsList[1]},
            { ColorsList[2].Key, ColorsList[2]},
            { ColorsList[3].Key, ColorsList[3]},
            { ColorsList[4].Key, ColorsList[4]},
            { ColorsList[5].Key, ColorsList[5]},
            { ColorsList[6].Key, ColorsList[6]},
            { ColorsList[7].Key, ColorsList[7]},
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
            if ( Colors.ContainsKey( key ) )
            {
                this.Key = Colors[key].Key;
                this.Color = Colors[key].Color;
                this.DarkColor1 = Colors[key].DarkColor1;
                this.DarkColor2 = Colors[key].DarkColor2;
            }
            else
            {
                this.Key = NoteColor.DEFAULT.Key;
                this.Color = NoteColor.DEFAULT.Color;
                this.DarkColor1 = NoteColor.DEFAULT.DarkColor1;
                this.DarkColor2 = NoteColor.DEFAULT.DarkColor2;
            }
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
            this.DarkColor1 = darkColor1;
            this.DarkColor2 = darkColor2;
        }

        public static NoteColor Random()
        {
            int index = new Random().Next( 1, Colors.Count );

            int count = 0;
            foreach ( KeyValuePair< string, NoteColor > item in Colors )
            {
                count++;
                if ( count == index )
                    return item.Value;
            }

            return NoteColor.DEFAULT;
        }

        public NoteColor Next()
        {
            int pos = ColorsList.FindIndex(n => n.Key == this.Key);
            int nextPos = pos >= ColorsList.Count - 1 ? 0 : pos + 1;

            return ColorsList[nextPos];
        }

        public NoteColor Previous()
        {
            int pos = ColorsList.FindIndex(n => n.Key == this.Key);
            int previousPos = pos <= 1 ? 0 : pos - 1;

            return ColorsList[previousPos];
        }

        public override string ToString()
        {
            return this.Color;
        }
    }
}
