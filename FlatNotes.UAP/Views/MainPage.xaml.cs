using FlatNotes.Utils;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Views
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<NavLink> _navLinks = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "Back", Symbol = Symbol.Back  },
            new NavLink() { Label = "Notes", Symbol = Symbol.Document  },
            new NavLink() { Label = "Archived", Symbol = Symbol.Delete },
            new NavLink() { Label = "Settings", Symbol = Symbol.Setting },
        };

        public ObservableCollection<NavLink> NavLinks { get { return _navLinks; } }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void MenuToggleButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }
    }
}
