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
        public static readonly DependencyProperty OpenSplitViewProperty = DependencyProperty.Register("OpenSplitView", typeof(bool), typeof(MainPage), new PropertyMetadata(false, OnOpenSplitViewPropertyChanged));
        public bool OpenSplitView { get { return (bool)GetValue(OpenSplitViewProperty); } set { SetValue(OpenSplitViewProperty, (value as bool?) == true); } }

        public static readonly DependencyProperty IsArchivedModeProperty = DependencyProperty.Register("IsArchivedMode", typeof(bool), typeof(MainPage), new PropertyMetadata(false, (d, e) =>
        {
            (d as MainPage).NotesPageNameButton.FontWeight = (d as MainPage).IsArchivedMode ? Windows.UI.Text.FontWeights.Normal : Windows.UI.Text.FontWeights.Bold;
            (d as MainPage).ArchivedNotesPageNameButton.FontWeight = (d as MainPage).IsArchivedMode ? Windows.UI.Text.FontWeights.Bold : Windows.UI.Text.FontWeights.Normal;
        }));
        public bool IsArchivedMode { get { return (bool)GetValue(IsArchivedModeProperty); } set { SetValue(IsArchivedModeProperty, (value as bool?) == true); } }

        public static readonly DependencyProperty PageMainBackgroundBrushProperty = DependencyProperty.Register("PageMainBackgroundBrush", typeof(SolidColorBrush), typeof(MainPage), new PropertyMetadata(new SolidColorBrush(App.MainColor)));
        public SolidColorBrush PageMainBackgroundBrush { get { return (SolidColorBrush)GetValue(PageMainBackgroundBrushProperty); } set { SetValue(PageMainBackgroundBrushProperty, value); } }
        
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

            Loading += MainPage_Loading;
            Loaded += MainPage_Loaded;
            Unloaded += (s, e) => OnUnloaded();
        }

        private void MainPage_Loading(FrameworkElement sender, object args)
        {
            UpdateStatusBarAndCommandBarColors();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            notesPage.NoteOpened += NotesPage_NoteOpened;
            notesPage.NoteClosed += NotesPage_NoteClosed;
            this.SettingsPage.viewModel.CloseModal += ViewModel_CloseModal;
        }

        private void OnUnloaded()
        {
            notesPage.NoteOpened -= NotesPage_NoteOpened;
            notesPage.NoteClosed -= NotesPage_NoteClosed;
            this.SettingsPage.viewModel.CloseModal -= ViewModel_CloseModal;
        }

        private void NotesPage_NoteOpened(object sender, EventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void NotesPage_NoteClosed(object sender, EventArgs e)
        {
            UpdateStatusBarAndCommandBarColors();
            if (!App.RootFrame.CanGoBack) SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private void ViewModel_CloseModal(object sender, EventArgs e)
        {
            CloseModal();
        }

        public void UpdateStatusBarAndCommandBarColors()
        {
            if (IsArchivedMode)
            {
                var statusBarBackgroundColor = Color.FromArgb(0xff, 0x44, 0x59, 0x63);//.Add(Color.FromArgb(0x10, 0, 0, 0));
                var statusBarForegroundColor = Color.FromArgb(0xD0, 0xff, 0xff, 0xff);
                App.ChangeStatusBarColor(statusBarBackgroundColor, statusBarForegroundColor, ElementTheme.Dark);

                PageMainBackgroundBrush = new SolidColorBrush(statusBarBackgroundColor);
            }
            else
            {
                PageMainBackgroundBrush = new SolidColorBrush(App.MainColor);
                App.ResetStatusBar();
            }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            App.RootFrame.Background = this.Background;

            //received a note via parameter (from secondary tile)
            if (RedirectToNote != null)
            {
                await Task.Delay(0200);
                notesPage.OpenNote(RedirectToNote);
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
                if (e.Parameter is MainPageNavigationArgument)
                {
                    var parameters = e.Parameter as MainPageNavigationArgument;
                    IsArchivedMode = parameters.IsArchivedMode;
                    if (!String.IsNullOrEmpty(parameters.NoteId)) RedirectToNote = e.NavigationMode == NavigationMode.New ? NotificationsManager.TryToGetNoteFromNavigationArgument(parameters.NoteId) : null;
                }
                else
                {
                    //fallback to default navigation parameter handler (e.g: live tile will send ?noteId=xxx as a string)
                    //note parameter
                    RedirectToNote = e.NavigationMode == NavigationMode.New ? NotificationsManager.TryToGetNoteFromNavigationArgument(e.Parameter.ToString()) : null;
                }
            }

            if (!IsArchivedMode) App.RootFrame.BackStack.Clear();
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
