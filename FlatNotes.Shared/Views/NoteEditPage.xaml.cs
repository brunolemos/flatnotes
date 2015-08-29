using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
#if WINDOWS_PHONE_APP
    public sealed partial class NoteEditPage : Page, IFileOpenPickerContinuable
#else
    public sealed partial class NoteEditPage : Page
#endif
    {
        public NoteEditViewModel viewModel { get { return _viewModel; } }
        private static NoteEditViewModel _viewModel = NoteEditViewModel.Instance;

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        private static Brush previousBackground;
        private bool checklistChanged = false;

#if WINDOWS_PHONE_APP
        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame);
        partial void DisableSwipeFeature(FrameworkElement element);
#endif

        public NoteEditPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            this.Loaded += (s, e) => OnColorChanged();

#if WINDOWS_PHONE_APP
            //Color Picker WP81
            ColorPickerAppBarToggleButton.Checked += (s, _e) => NoteColorPicker.Open();
            ColorPickerAppBarToggleButton.Unchecked += (s, _e) => NoteColorPicker.Close();
            NoteColorPicker.Opened += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = true; };
            NoteColorPicker.Closed += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = false; };
#endif
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;

            if (e.NavigationParameter != null && e.NavigationParameter is Note)
                viewModel.Note = e.NavigationParameter as Note;
            else
                viewModel.Note = new Note();

            viewModel.Note.Changed = false;
            viewModel.Note.Images.CollectionChanged += Images_CollectionChanged;
            viewModel.Note.Checklist.CollectionChanged += Checklist_CollectionChanged;
            viewModel.Note.Checklist.CollectionItemChanged += Checklist_CollectionItemChanged;
            viewModel.Note.PropertyChanged += OnNotePropertyChanged;

            previousBackground = App.RootFrame.Background;
            App.RootFrame.Background = new SolidColorBrush().fromHex(viewModel.Note.Color.Color);

            //Color Picker
            NoteColorPicker.SelectedNoteColor = viewModel.Note.Color;
            NoteColorPicker.NoteColorChanged += (s, _e) => { viewModel.Note.Color = _e.NoteColor; };
        }

        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState BEGIN {0}", NoteEditViewModel.CurrentNoteBeingEdited);
            App.RootFrame.Background = previousBackground;

            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 1");
            //prevent from losing changes when navigating with textbox focused
            this.CommandBar.Focus(FocusState.Programmatic);
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 2");
            this.CommandBar.IsOpen = false;
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 3");
            //await Task.Delay(0200);
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 4");

            //deleted
            if (viewModel.Note == null) return;
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 5");

            //remove change binding
            viewModel.Note.Images.CollectionChanged -= Images_CollectionChanged;
            viewModel.Note.Checklist.CollectionChanged -= Checklist_CollectionChanged;
            viewModel.Note.Checklist.CollectionItemChanged -= Checklist_CollectionItemChanged;
            viewModel.Note.PropertyChanged -= OnNotePropertyChanged;
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 6");

            //trim
            viewModel.Note.Trim();
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 7");

            //update tile
            if (viewModel.Note.Changed) await TileManager.UpdateNoteTileIfExists(viewModel.Note, AppSettings.Instance.TransparentNoteTile);
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 8");

            //save or remove if empty
            if (viewModel.Note.Changed || viewModel.Note.IsEmpty())
                await AppData.CreateOrUpdateNote(viewModel.Note);
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 9");

            //checklist changed (fix cache problem with converter)
            if (checklistChanged) viewModel.Note.NotifyChanges();
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState 10");

            //NoteEditViewModel.CurrentNoteBeingEdited = null;
            //viewModel.Note = null;
            System.Diagnostics.Debug.WriteLine("NavigationHelper_SaveState END {0}", NoteEditViewModel.CurrentNoteBeingEdited);
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Note" && viewModel.Note != null)
            {
                //viewModel.Note.PropertyChanged -= OnNotePropertyChanged;
                //viewModel.Note.PropertyChanged += OnNotePropertyChanged;

                OnColorChanged();
                UpdateIsPinnedStatus();
            }
        }

        private void OnNotePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsPinned" || e.PropertyName == "CanPin")
                UpdateIsPinnedStatus();
            else if (e.PropertyName == "Color")
                OnColorChanged();
        }

        private void OnColorChanged()
        {
            if (viewModel.Note == null) return;
            App.ChangeStatusBarColor(new Color().FromHex(viewModel.Note.Color.DarkColor2));

            try
            {
                var style = new Style(typeof(FlyoutPresenter));
                //style.Setters.Add(new Setter(FlyoutPresenter.BackgroundProperty, new Color().FromHex(viewModel.Note.Color.DarkColor2)));
                style.Setters.Add(new Setter(FlyoutPresenter.BorderThicknessProperty, new Thickness(0)));

                ColorPickerAppBarToggleButton.Flyout.SetValue(Flyout.FlyoutPresenterStyleProperty, style);
            }
            catch (Exception)
            {
            }
        }

        private void UpdateIsPinnedStatus(bool? forceStatus = null)
        {
            if (viewModel.Note == null) return;
            bool isPinned = forceStatus != null ? (bool)forceStatus : viewModel.Note.IsPinned;

            if (isPinned)
            {
                TogglePinAppBarButton.Icon = new SymbolIcon(Symbol.UnPin);
                TogglePinAppBarButton.Command = viewModel.UnpinCommand;
                TogglePinAppBarButton.Label = ResourceLoader.GetForCurrentView().GetString("Unpin");
            }
            else
            {
                TogglePinAppBarButton.Icon = new SymbolIcon(Symbol.Pin);
                TogglePinAppBarButton.Command = viewModel.PinCommand;
                TogglePinAppBarButton.Label = ResourceLoader.GetForCurrentView().GetString("Pin");
                TogglePinAppBarButton.IsEnabled = viewModel.Note.CanPin;
            }
        }

        private void Images_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Images_CollectionChanged");
            viewModel.Note.Touch();
        }

        private void Checklist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("Checklist_CollectionChanged");
            checklistChanged = true;
            viewModel.Note.Touch();

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                try
                {
                    var list = (object[])e.OldItems.SyncRoot;
                    var checklistItem = list[0] as ChecklistItem;

                    if (checklistItem != null)
                        AppData.DB.Delete<ChecklistItem>(checklistItem.ID);
                }
                catch (Exception)
                {
                }
            }
        }

        private void Checklist_CollectionItemChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Checklist_CollectionItemChanged");
            checklistChanged = true;
            viewModel.Note.Touch();
        }

        private void NoteChecklistListView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            NoteChecklistListView.ReorderMode = ListViewReorderMode.Enabled;
        }

#if WINDOWS_PHONE_APP
        //swipe feature
        private void OnChecklistItemLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NoteChecklistListView;

            //workaround to fix a bug
            element.Opacity = 1;

            if (viewModel.ReorderMode != ListViewReorderMode.Enabled)
                EnableSwipeFeature(element, referenceFrame);

            enableSwipeEventHandlers[element] = (s, _e) => { EnableSwipeFeature(element, referenceFrame); };
            disableSwipeEventHandlers[element] = (s, _e) => { DisableSwipeFeature(element); };

            viewModel.ReorderModeDisabled += enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled += disableSwipeEventHandlers[element];
        }

        private void OnChecklistItemUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            viewModel.ReorderModeDisabled -= enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled -= disableSwipeEventHandlers[element];

            DisableSwipeFeature(element);
        }
#endif

        private void DeleteNoteImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            viewModel.TempNoteImage = (e.OriginalSource as FrameworkElement).DataContext as NoteImage;
            viewModel.DeleteNoteImageCommand.Execute(viewModel.TempNoteImage);
        }

        private void NoteImageContainer_Holding(object sender, HoldingRoutedEventArgs e)
        {
            ShowNoteImageFlyout((FrameworkElement)sender);
        }

#if WINDOWS_UWP
        private void NoteImageContainer_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowNoteImageFlyout((FrameworkElement)sender);
        }
#endif

        private void ShowNoteImageFlyout(FrameworkElement element)
        {
            if (element == null) return;
            Flyout.ShowAttachedFlyout(element);
        }

#if WINDOWS_PHONE_APP
        public void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            viewModel.HandleImageFromFilePicker(args.Files);
        }
#endif
    }
}
