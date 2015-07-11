using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
#if WINDOWS_PHONE_APP
    public partial class MainPage : Page, IFileOpenPickerContinuable
#else
    public partial class MainPage : Page
#endif
    {
        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = new MainViewModel();

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        private Note RedirectToNote = null;
        private static NoteSwipeFeature noteSwipeFeature = new NoteSwipeFeature();

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            
            this.Loaded += (s, e) => App.ResetStatusBar();

            this.Loaded += (s, e) => EnableReorderFeature();
            this.Unloaded += (s, e) => DisableReorderFeature();
        }

        partial void EnableReorderFeature();
        partial void DisableReorderFeature();

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("MainPage");

            App.RootFrame.Background = LayoutRoot.Background;

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
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomMetric(1, AppData.Notes.Count);
            GoogleAnalytics.EasyTracker.GetTracker().SetCustomMetric(2, AppData.ArchivedNotes.Count);

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
        
        private async void OnNoteTapped(object sender, TappedRoutedEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            if (viewModel.ReorderMode == ListViewReorderMode.Enabled) return;
            #endif

            Note note = (e.OriginalSource as FrameworkElement).DataContext as Note;
            if (note == null) return;

            //it can be trimmed, so get the original
            Note originalNote = AppData.Notes.Where<Note>(n => n.ID == note.ID).FirstOrDefault();
            if (originalNote == null)
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(string.Format("Failed to load tapped note ({0})", Newtonsoft.Json.JsonConvert.SerializeObject(AppData.Notes)), false);
                return;
            }

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), originalNote);
            });
        }

        //swipe feature
        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NotesListView;

            if(viewModel.ReorderMode != ListViewReorderMode.Enabled)
                noteSwipeFeature.EnableSwipeFeature(element, referenceFrame);

            noteSwipeFeature.enableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.EnableSwipeFeature(element, referenceFrame); };
            noteSwipeFeature.disableSwipeEventHandlers[element] = (s, _e) => { noteSwipeFeature.DisableSwipeFeature(element); };

            viewModel.ReorderModeDisabled += noteSwipeFeature.enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled += noteSwipeFeature.disableSwipeEventHandlers[element];
#endif
        }

        private void OnNoteUnloaded(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            FrameworkElement element = sender as FrameworkElement;

            if (noteSwipeFeature.enableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeDisabled -= noteSwipeFeature.enableSwipeEventHandlers[element];
            if (noteSwipeFeature.disableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeEnabled -= noteSwipeFeature.disableSwipeEventHandlers[element];

            noteSwipeFeature.enableSwipeEventHandlers.Remove(element);
            noteSwipeFeature.disableSwipeEventHandlers.Remove(element);

            noteSwipeFeature.DisableSwipeFeature(element);
#endif
        }

#if WINDOWS_PHONE_APP
        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count <= 0) return;

            Note note = new Note();
            NoteImage noteImage = new NoteImage();
            string error = "";

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
            catch (Exception e) { error = e.Message; }
            
            if(!String.IsNullOrEmpty(error))
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("Failed to load image ({0})", error), false);
                await (new MessageDialog("Failed to save image. Try again.", "Sorry")).ShowAsync();

                return;
            }

            Frame.Navigate(typeof(NoteEditPage), note);

            //save
            await AppData.CreateOrUpdateNote(note);
        }
#endif
    }
}
