using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
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

            this.SettingsPage.viewModel.CloseModal += (s, e) => CloseModal();
            
            NotesPage.NoteOpened += (s, e) => SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            NotesPage.NoteClosed += (s, e) => { App.ResetStatusBar(); SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed; };
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
            bool showContentOverlayAnimation = (e.NewValue as bool?) == true;

            if (showContentOverlayAnimation) mainPage.OpenModal(); else mainPage.CloseModal();
        }

        private void OpenModal()
        {
            OpenSplitView = true;
            ShowContentOverlayAnimation.Begin();
        }

        private void CloseModal()
        {
            OpenSplitView = false;
            HideContentOverlayAnimation.Begin();
        }

        private void OnNoteOpening(object sender, EventArgs e)
        {
            ShowContentOverlayAnimation.Begin();
        }

        private void OnNoteClosed(object sender, EventArgs e)
        {
            HideContentOverlayAnimation.Begin();
        }
    }
}
