using System;
using System.Collections.Generic;
using System.Linq;
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
        public string Key { get; private set; }

        public string Color { get; private set; }

        public static readonly NoteColor DEFAULT = new NoteColor("DEFAULT", ((SolidColorBrush)App.Current.Resources["KeepNoteDefaultBrush"]).Color.ToString());
        public static readonly NoteColor RED = new NoteColor("RED", ((SolidColorBrush)App.Current.Resources["KeepNoteRedBrush"]).Color.ToString());
        public static readonly NoteColor ORANGE = new NoteColor("ORANGE", ((SolidColorBrush)App.Current.Resources["KeepNoteOrangeBrush"]).Color.ToString());
        public static readonly NoteColor YELLOW = new NoteColor("YELLOW", ((SolidColorBrush)App.Current.Resources["KeepNoteYellowBrush"]).Color.ToString());
        public static readonly NoteColor GREEN = new NoteColor("GREEN", ((SolidColorBrush)App.Current.Resources["KeepNoteGreenBrush"]).Color.ToString());
        public static readonly NoteColor TEAL = new NoteColor("TEAL", ((SolidColorBrush)App.Current.Resources["KeepNoteTealBrush"]).Color.ToString());
        public static readonly NoteColor BLUE = new NoteColor("BLUE", ((SolidColorBrush)App.Current.Resources["KeepNoteBlueBrush"]).Color.ToString());
        public static readonly NoteColor GRAY = new NoteColor("GRAY", ((SolidColorBrush)App.Current.Resources["KeepNoteGrayBrush"]).Color.ToString());

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
        }

        public NoteColor( string key )
        {
            if ( Colors.ContainsKey( key ) )
            {
                this.Key = Colors[key].Key;
                this.Color = Colors[key].Color;
            }
            else
            {
                this.Key = NoteColor.DEFAULT.Key;
                this.Color = NoteColor.DEFAULT.Color;
            }
        }

        private NoteColor(string key, string color)
        {
            this.Key = key;
            this.Color = color;
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
