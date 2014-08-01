using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Windows.UI.Xaml.Media;

namespace Keep.Models
{
    [DataContract]
    public sealed class NoteColor
    {
        [DataMember]
        public string Key { get; private set; }

        public string Color { get; private set; }

        public static readonly NoteColor DEFAULT = new NoteColor( "DEFAULT", ((SolidColorBrush)App.Current.Resources["KeepNoteDefaultBrush"]).Color.ToString() );
        public static readonly NoteColor RED = new NoteColor( "RED", ((SolidColorBrush)App.Current.Resources["KeepNoteRedBrush"]).Color.ToString() );
        public static readonly NoteColor ORANGE = new NoteColor("ORANGE", ((SolidColorBrush)App.Current.Resources["KeepNoteOrangeBrush"]).Color.ToString() );
        public static readonly NoteColor YELLOW = new NoteColor("YELLOW", ((SolidColorBrush)App.Current.Resources["KeepNoteYellowBrush"]).Color.ToString() );
        public static readonly NoteColor GREEN = new NoteColor("GREEN", ((SolidColorBrush)App.Current.Resources["KeepNoteGreenBrush"]).Color.ToString() );
        public static readonly NoteColor TEAL = new NoteColor("TEAL", ((SolidColorBrush)App.Current.Resources["KeepNoteTealBrush"]).Color.ToString() );
        public static readonly NoteColor BLUE = new NoteColor("BLUE", ((SolidColorBrush)App.Current.Resources["KeepNoteBlueBrush"]).Color.ToString() );
        public static readonly NoteColor GRAY = new NoteColor("GRAY", ((SolidColorBrush)App.Current.Resources["KeepNoteGrayBrush"]).Color.ToString());

        private static readonly Dictionary<string, NoteColor> Colors = new Dictionary<string, NoteColor>()
        {
            { NoteColor.DEFAULT.Key, NoteColor.DEFAULT },
            { NoteColor.RED.Key, NoteColor.RED },
            { NoteColor.ORANGE.Key, NoteColor.ORANGE },
            { NoteColor.YELLOW.Key, NoteColor.YELLOW },
            { NoteColor.GREEN.Key, NoteColor.GREEN },
            { NoteColor.TEAL.Key, NoteColor.TEAL },
            { NoteColor.BLUE.Key, NoteColor.BLUE },
            { NoteColor.GRAY.Key, NoteColor.GRAY },
        };

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

        private NoteColor( string key, string color )
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

        public override string ToString()
        {
            return this.Color;
        }
    }
}
