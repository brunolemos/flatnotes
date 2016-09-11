using FlatNotes.Common;
using FlatNotes.Events;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Views
{
    public partial class NotesPage : Page
    {
        public static readonly DependencyProperty IsPopupLightDismissEnabledProperty = DependencyProperty.Register("IsPopupLightDismissEnabled", typeof(bool), typeof(NotesPage), new PropertyMetadata(true));
        public bool IsPopupLightDismissEnabled { get { return (bool)GetValue(IsPopupLightDismissEnabledProperty); } set { SetValue(IsPopupLightDismissEnabledProperty, value); } }

        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = MainViewModel.Instance;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public static event EventHandler EnableLightDismissReceived;
        public static event EventHandler DisableLightDismissReceived;

        public event EventHandler NoteOpening;
        public event EventHandler NoteOpened;
        public event EventHandler NoteClosed;

        public NotesPage()
        {
            this.InitializeComponent();

            viewModel.ShowNote = OpenNote;

            Loading += OnLoading;

#if WINDOWS_PHONE_APP
            Loaded += (s, e) => EnableReorderFeature();
            Unloaded += (s, e) => DisableReorderFeature();
#endif

            EnableLightDismissReceived += (s, e) => { IsPopupLightDismissEnabled = true; };
            DisableLightDismissReceived += (s, e) => { IsPopupLightDismissEnabled = false; };
        }

#if WINDOWS_PHONE_APP
        partial void EnableReorderFeature();
        partial void DisableReorderFeature();
#endif

        private void OnLoading(FrameworkElement sender, object args)
        {
            viewModel.ReorderMode = ListViewReorderMode.Disabled;
            if (viewModel.Notes == null || viewModel.Notes.Count <= 0) viewModel.Notes = AppData.Notes;
        }

        private async void OnNoteClick(object sender, ItemClickEventArgs e)
        {
            Note note = e.ClickedItem as Note;
            if (note == null) return;

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OpenNote(note);
            });
        }

        private void OnItemsReordered(object sender, Events.ItemsReorderedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("OnItemsReordered from {0} to {1}", e.OldItemIndex, e.NewItemIndex);
            if (e.OldItemIndex < 0 || e.NewItemIndex < 0) return;
            if (e.OldItemIndex > viewModel.Notes.Count || e.NewItemIndex > viewModel.Notes.Count) return;

            viewModel.Notes.Move(e.OldItemIndex, e.NewItemIndex);

            int pos = viewModel.Notes.Count - 1;
            foreach (var note in viewModel.Notes)
            {
                note.Order = pos;
                pos--;
            }

            AppData.LocalDB.UpdateAll(viewModel.Notes);
            AppData.RoamingDB.UpdateAll(viewModel.Notes);
        }

        public void OpenNote(object parameter)
        {
            UpdateNotePopupSizeAndPosition();
            NoteOpening?.Invoke(this, EventArgs.Empty);
            ShowPopupAnimation.Begin();

            NoteFrame.Navigate(typeof(NoteEditPage), parameter);
            
            NotePopup.IsOpen = true;
            NoteOpened?.Invoke(this, new GenericEventArgs(parameter));
        }

        public void CloseNote()
        {
            NotePopup.IsOpen = false;
        }

        private void NotePopup_Opened(object sender, object e)
        {
            UpdateNotePopupSizeAndPosition();
            NoteOpened?.Invoke(this, EventArgs.Empty);
        }

        private void NotePopup_Closed(object sender, object e)
        {
            NoteClosed?.Invoke(this, EventArgs.Empty);

            HidePopupAnimation.Completed += (s2, e2) => {
                NotePopup.IsOpen = false;
                while (NoteFrame.CanGoBack) NoteFrame.GoBack();
            };

            HidePopupAnimation.Begin();
        }

        // needed because the ActualWidth and ActualHeight properties dont post updates when its changed
        // see: https://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.frameworkelement.actualwidth
        private void UpdateNotePopupSizeAndPosition()
        {
            if (ActualWidth < 768)
            {
                NotePopup.HorizontalOffset = 0;
                NotePopup.VerticalOffset = -48;// CommandBar Height

                NoteFrame.Width = ActualWidth - NotePopup.HorizontalOffset;
                NoteFrame.Height = ActualHeight - NotePopup.VerticalOffset;
                NoteFrame.ClearValue(FrameworkElement.MaxHeightProperty);
            }
            else
            {
                var ttv = NoteQuickBox.TransformToVisual(Content);
                Point point = ttv.TransformPoint(new Point(0, 0));

                NotePopup.HorizontalOffset = point.X;
                NotePopup.VerticalOffset = point.Y;

                NoteFrame.Width = NoteQuickBox.ActualWidth;
                NoteFrame.ClearValue(FrameworkElement.HeightProperty);

                ttv = NoteQuickBox.TransformToVisual(ContentRoot);
                point = ttv.TransformPoint(new Point(0, 0));

                NoteFrame.MaxHeight = Math.Max(0, ContentRoot.ActualHeight - NotePopup.VerticalOffset - point.Y);
            }
        }

#if WINDOWS_PHONE_APP
        private void OnCreateNoteTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            viewModel.CreateTextNote();
        }
#endif

        public static void EnablePopupLightDismiss()
        {
            EnableLightDismissReceived?.Invoke(null, EventArgs.Empty);
        }

        public static void DisablePopupLightDismiss()
        {
            DisableLightDismissReceived?.Invoke(null, EventArgs.Empty);
        }
    }
}
