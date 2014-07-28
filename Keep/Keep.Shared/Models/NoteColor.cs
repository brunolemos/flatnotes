using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Keep.Models
{
    [DataContract]
    public sealed class NoteColor
    {
        [DataMember]
        public string Key { get; private set; }

        public string Color { get; private set; }

        public static readonly NoteColor DEFAULT = new NoteColor( "DEFAULT", App.Current.Resources["KeepNoteDefaultBrush"].ToString() );
        public static readonly NoteColor RED = new NoteColor( "RED", App.Current.Resources["KeepNoteRedBrush"].ToString() );
        public static readonly NoteColor ORANGE = new NoteColor("ORANGE", App.Current.Resources["KeepNoteOrangeBrush"].ToString());
        public static readonly NoteColor YELLOW = new NoteColor("YELLOW", App.Current.Resources["KeepNoteYellowBrush"].ToString());
        public static readonly NoteColor GREEN = new NoteColor("GREEN", App.Current.Resources["KeepNoteGreenBrush"].ToString());
        public static readonly NoteColor TEAL = new NoteColor("TEAL", App.Current.Resources["KeepNoteTealBrush"].ToString());
        public static readonly NoteColor BLUE = new NoteColor("BLUE", App.Current.Resources["KeepNoteBlueBrush"].ToString());
        public static readonly NoteColor GRAY = new NoteColor("GRAY", App.Current.Resources["KeepNoteGrayBrush"].ToString());

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
