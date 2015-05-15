using Windows.UI.Xaml.Controls;

namespace FlatNotes.Utils
{
    public class NavLink
    {
        public string Label { get; set; } = "";
        public Symbol Symbol { get; set; } = Symbol.Emoji;
        public bool IsSeparator { get; set; } = false;
    }
}
