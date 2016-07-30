using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public partial class MainPage : Page
    {
        public static readonly DependencyProperty OpenSplitViewProperty = DependencyProperty.Register("OpenSplitView", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnOpenSplitViewPropertyChanged));
        public bool OpenSplitView { get { return (bool)GetValue(OpenSplitViewProperty); } set { SetValue(OpenSplitViewProperty, (value as bool?) == true); } }

        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = MainViewModel.Instance;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        private Note RedirectToNote = null;

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.SettingsPage.viewModel.CloseModal += CloseModal;
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.ResetStatusBar();
            App.RootFrame.Background = this.Background;

            //received a note via parameter (from secondary tile)
            if (RedirectToNote != null)
            {
                await Task.Delay(0200);
                Frame.Navigate(typeof(NoteEditPage), RedirectToNote);
                RedirectToNote = null;

                return;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            RedirectToNote = null;
        }

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //has parameters
            if (e.Parameter != null && !String.IsNullOrEmpty(e.Parameter.ToString()))
            {
                //note parameter
                RedirectToNote = e.NavigationMode == NavigationMode.New ? TileManager.TryToGetNoteFromNavigationArgument(e.Parameter.ToString()) : null;
            }

            Frame.BackStack.Clear();
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }
        #endregion

        static void OnOpenSplitViewPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MainPage mainPage = obj as MainPage;
            bool showSplitViewOverlay = (e.NewValue as bool?) == true;

            if (showSplitViewOverlay) mainPage.ShowSplitViewOverlay.Begin();  else mainPage.HideSplitViewOverlay.Begin();
        }

        private void CloseModal(object sender, EventArgs e)
        {
            OpenSplitView = false;
        }

        private void OnNoteOpening(object sender, EventArgs e)
        {
            ShowSplitViewOverlay.Begin();
        }

        private void OnNoteClosed(object sender, EventArgs e)
        {
            HideSplitViewOverlay.Begin();
        }
    }
}
