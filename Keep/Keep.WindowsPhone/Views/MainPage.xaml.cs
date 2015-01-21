using System;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Popups;

namespace Keep.Views
{
    public sealed partial class MainPage : Page, IFileOpenPickerContinuable
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public MainViewModel viewModel { get { return (MainViewModel)DataContext; } }

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.Loaded += (s, e) => EnableReorderFeature();
            this.Unloaded += (s, e) => DisableReorderFeature();
        }

        partial void EnableReorderFeature();
        partial void DisableReorderFeature();

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame);
        partial void DisableSwipeFeature(FrameworkElement element);

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("MainPage");

            //App.ChangeStatusBarColor();
            App.RootFrame.Background = LayoutRoot.Background;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomMetric(1, AppData.Notes.Count);
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomMetric(2, AppData.ArchivedNotes.Count);
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }


        #endregion

        private void OnNoteTapped(object sender, TappedRoutedEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            if (viewModel.ReorderMode == ListViewReorderMode.Enabled) return;
            #endif

            Note note = (e.OriginalSource as FrameworkElement).DataContext as Note;
            if (note == null) return;

            //it can be trimmed, so get the original
            Note originalNote = AppData.Notes.Where<Note>(n => n.ID == note.ID).FirstOrDefault();
            if (originalNote == null) GoogleAnalytics.EasyTracker.GetTracker().SendException(string.Format("Failed to load tapped note ({0})", note.GetContent()), false);

            App.RootFrame.Navigate(typeof(NoteEditPage), originalNote);
        }

        //swipe feature
        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NotesListView;

            if(viewModel.ReorderMode != ListViewReorderMode.Enabled)
                EnableSwipeFeature(element, referenceFrame);

            enableSwipeEventHandlers[element] = (s, _e) => { EnableSwipeFeature(element, referenceFrame); };
            disableSwipeEventHandlers[element] = (s, _e) => { DisableSwipeFeature(element); };

            viewModel.ReorderModeDisabled += enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled += disableSwipeEventHandlers[element];
        }

        private void OnNoteUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if(enableSwipeEventHandlers[element] != null) viewModel.ReorderModeDisabled -= enableSwipeEventHandlers[element];
            if(disableSwipeEventHandlers[element] != null) viewModel.ReorderModeEnabled -= disableSwipeEventHandlers[element];

            DisableSwipeFeature(element);
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                Note note = new Note();
                NoteImage noteImage = new NoteImage();

                try
                {
                    foreach (var file in args.Files)
                    {
                        Debug.WriteLine("Picked photo: " + file.Path);

                        StorageFile savedImage = await AppSettings.Instance.SaveImage(file, note.ID, noteImage.ID);

                        var imageProperties = await savedImage.Properties.GetImagePropertiesAsync();
                        noteImage.URL = savedImage.Path;
                        noteImage.Size = new Size(imageProperties.Width, imageProperties.Height);

                        note.Images.Add(noteImage);
                        break;
                    }
                }
                catch (Exception e)
                {
                    GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("Failed to load image ({0})", e.Message), false);
                    await (new MessageDialog("Failed to save image. Try again.", "Sorry")).ShowAsync();

                    return;
                }

                Frame.Navigate(typeof(NoteEditPage), note);

                //save
                await AppData.CreateOrUpdateNote(note);
            }
        }
    }
}
