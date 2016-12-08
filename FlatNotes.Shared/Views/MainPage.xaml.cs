using FlatNotes.Controls;
using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public partial class MainPage : Page
    {
        public static readonly DependencyProperty RedirectToNoteProperty = DependencyProperty.Register("RedirectToNote", typeof(Note), typeof(MainPage), new PropertyMetadata(null));
        public Note RedirectToNote { get { return (Note)GetValue(RedirectToNoteProperty); } set { SetValue(RedirectToNoteProperty, value); } }

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.RootFrame.Background = this.Background;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            RedirectToNote = null;
        }

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.ResetStatusBar();

            //has parameters
            if (e.Parameter != null && !String.IsNullOrEmpty(e.Parameter.ToString()))
            {
                if (e.Parameter is MainPageNavigationArgument)
                {
                    var parameters = e.Parameter as MainPageNavigationArgument;
                    if (!String.IsNullOrEmpty(parameters.NoteId)) RedirectToNote = e.NavigationMode == NavigationMode.New ? NotificationsManager.TryToGetNoteFromNavigationArgument(parameters.NoteId) : null;
                }
                else
                {
                    //fallback to default navigation parameter handler (e.g: live tile will send ?noteId=xxx as a string)
                    //note parameter
                    RedirectToNote = e.NavigationMode == NavigationMode.New ? NotificationsManager.TryToGetNoteFromNavigationArgument(e.Parameter.ToString()) : null;
                }
            }

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion
    }
}
