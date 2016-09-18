using FlatNotes.Common;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Views
{
    public partial class ArchivedNotesPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public ArchivedNotesPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
        }
    }
}
