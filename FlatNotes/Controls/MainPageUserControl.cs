using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using FlatNotes.Views;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Controls
{
    public partial class MainPageUserControl : UserControl
    {
        public static readonly DependencyProperty RedirectToNoteProperty = DependencyProperty.Register("RedirectToNote", typeof(Note), typeof(MainPageUserControl), new PropertyMetadata(null));
        public Note RedirectToNote { get { return (Note)GetValue(RedirectToNoteProperty); } set { SetValue(RedirectToNoteProperty, value); } }

        public static readonly DependencyProperty OpenSplitViewProperty = DependencyProperty.Register("OpenSplitView", typeof(bool), typeof(MainPageUserControl), new PropertyMetadata(false, OnOpenSplitViewPropertyChanged));
        public bool OpenSplitView { get { return (bool)GetValue(OpenSplitViewProperty); } set { SetValue(OpenSplitViewProperty, (value as bool?) == true); } }

        public NotesPage NotesPage { get { return notesPage; } }

        public static readonly DependencyProperty IsArchivedModeProperty = DependencyProperty.Register("IsArchivedMode", typeof(bool), typeof(MainPageUserControl), new PropertyMetadata(false, (d, e) =>
        {
            (d as MainPageUserControl).NotesPageNameButton.FontWeight = (d as MainPageUserControl).IsArchivedMode ? Windows.UI.Text.FontWeights.Normal : Windows.UI.Text.FontWeights.Bold;
            (d as MainPageUserControl).ArchivedNotesPageNameButton.FontWeight = (d as MainPageUserControl).IsArchivedMode ? Windows.UI.Text.FontWeights.Bold : Windows.UI.Text.FontWeights.Normal;
        }));
        public bool IsArchivedMode { get { return (bool)GetValue(IsArchivedModeProperty); } set { SetValue(IsArchivedModeProperty, (value as bool?) == true); } }

        public static readonly DependencyProperty PageMainBackgroundBrushProperty = DependencyProperty.Register("PageMainBackgroundBrush", typeof(SolidColorBrush), typeof(MainPageUserControl), new PropertyMetadata(new SolidColorBrush(App.MainColor)));
        public SolidColorBrush PageMainBackgroundBrush { get { return (SolidColorBrush)GetValue(PageMainBackgroundBrushProperty); } set { SetValue(PageMainBackgroundBrushProperty, value); } }
        
        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = MainViewModel.Instance;

        public MainPageUserControl()
        {
            this.InitializeComponent();

            Loading += MainPageUserControl_Loading;
            Loaded += MainPageUserControl_Loaded;
            Unloaded += (s, e) => OnUnloaded();
        }

        private void MainPageUserControl_Loading(FrameworkElement sender, object args)
        {
            UpdateStatusBarAndCommandBarColors();
        }

        private async void MainPageUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateStatusBarAndCommandBarColors();

            notesPage.NoteOpened += NotesPage_NoteOpened;
            notesPage.NoteClosed += NotesPage_NoteClosed;
            NoteEditPage.NoteUnloaded += NoteEditPage_NoteUnloaded;
            this.SettingsPage.viewModel.CloseModal += ViewModel_CloseModal;

            //received a note via parameter (from secondary tile)
            if (RedirectToNote != null)
            {
                await Task.Delay(0200);
                notesPage.OpenNote(RedirectToNote);
                RedirectToNote = null;

                return;
            }

            if (!IsArchivedMode) App.RootFrame.BackStack.Clear();
        }

        private void OnUnloaded()
        {
            RedirectToNote = null;

            notesPage.NoteOpened -= NotesPage_NoteOpened;
            notesPage.NoteClosed -= NotesPage_NoteClosed;
            NoteEditPage.NoteUnloaded -= NoteEditPage_NoteUnloaded;
            this.SettingsPage.viewModel.CloseModal -= ViewModel_CloseModal;
        }

        private void NotesPage_NoteOpened(object sender, EventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void NotesPage_NoteClosed(object sender, EventArgs e)
        {
        }

        private void NoteEditPage_NoteUnloaded(object sender, EventArgs e)
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

        static void OnOpenSplitViewPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            MainPageUserControl MainPageUserControl = obj as MainPageUserControl;
            bool showContentOverlayAnimation = (e.NewValue as bool?) == true;

            if (showContentOverlayAnimation) MainPageUserControl.OpenModal(); else MainPageUserControl.CloseModal();
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
            UpdateStatusBarAndCommandBarColors();
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
