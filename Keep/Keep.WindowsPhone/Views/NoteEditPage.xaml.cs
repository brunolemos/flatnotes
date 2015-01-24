﻿using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;
using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls.Primitives;

namespace Keep.Views
{
    public sealed partial class NoteEditPage : Page, IFileOpenPickerContinuable
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public NoteEditViewModel viewModel { get { return (NoteEditViewModel)DataContext; } }
        private static Brush previousBackground;

        private bool checklistChanged = false;

        public NoteEditPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //Color Picker
            ColorPickerAppBarToggleButton.Checked += (s, _e) => NoteColorPicker.Open();
            ColorPickerAppBarToggleButton.Unchecked += (s, _e) => NoteColorPicker.Close();
            NoteColorPicker.Opened += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = true; };
            NoteColorPicker.Closed += (s, _e) => { ColorPickerAppBarToggleButton.IsChecked = false; };
            NoteColorPicker.SelectionChanged += (s, _e) => { viewModel.Note.Color = _e.AddedItems[0] as NoteColor; };
        }

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame);
        partial void DisableSwipeFeature(FrameworkElement element);

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("NoteEditPage");

            //App.ChangeStatusBarColor(Colors.Black);

            if (e.NavigationParameter is Note)
                viewModel.Note = e.NavigationParameter as Note;

            if (viewModel.Note == null)
                viewModel.Note = new Note();

            viewModel.Note.Changed = false;
            viewModel.Note.Images.CollectionChanged += Images_CollectionChanged;
            viewModel.Note.Checklist.CollectionChanged += Checklist_CollectionChanged;
            viewModel.Note.Checklist.CollectionItemChanged += Checklist_CollectionItemChanged;

            previousBackground = App.RootFrame.Background;
            App.RootFrame.Background = new SolidColorBrush(new Color().FromHex(viewModel.Note.Color.Color));
        }

        private async void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            App.RootFrame.Background = previousBackground;

            //deleted
            if (viewModel.Note == null) return;

            //remove change binding
            viewModel.Note.Images.CollectionChanged -= Images_CollectionChanged;
            viewModel.Note.Checklist.CollectionChanged -= Checklist_CollectionChanged;
            viewModel.Note.Checklist.CollectionItemChanged -= Checklist_CollectionItemChanged;

            //trim
            viewModel.Note.Trim();

            //save
            if (viewModel.Note.Changed)
                await AppData.CreateOrUpdateNote(viewModel.Note);

            //checklist changed (fix cache problem with converter)
            if (checklistChanged) viewModel.Note.NotifyChanges();
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

        private void Images_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            viewModel.Note.Touch();
        }

        private void Checklist_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            checklistChanged = true;
            viewModel.Note.Touch();
        }

        private void Checklist_CollectionItemChanged(object sender, EventArgs e)
        {
            checklistChanged = true;
            viewModel.Note.Touch();
        }

        private void NoteChecklistListView_Holding(object sender, HoldingRoutedEventArgs e)
        {
            #if WINDOWS_PHONE_APP
            NoteChecklistListView.ReorderMode = ListViewReorderMode.Enabled;
            #endif
        }

        //swipe feature
        private void OnChecklistItemLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NoteChecklistListView;

            //workaround to fix a bug
            element.Opacity = 1;

            if(viewModel.ReorderMode != ListViewReorderMode.Enabled)
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

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count <= 0) return;

            string error = "";

            try
            {
                //delete old images
                await AppData.RemoveNoteImages(viewModel.Note.Images);

                //clear image list
                viewModel.Note.Images.Clear();

                //add new images
                foreach (var file in args.Files)
                {
                    Debug.WriteLine("Picked photo: " + file.Path);

                    NoteImage noteImage = new NoteImage();

                    StorageFile savedImage = await AppSettings.Instance.SaveImage(file, viewModel.Note.ID, noteImage.ID);

                    var imageProperties = await savedImage.Properties.GetImagePropertiesAsync();
                    noteImage.URL = savedImage.Path;
                    noteImage.Size = new Size(imageProperties.Width, imageProperties.Height);

                    viewModel.Note.Images.Add(noteImage);
                    break;
                }
            }
            catch (Exception e) { error = e.Message; }

            if(!String.IsNullOrEmpty(error))
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(String.Format("Failed to save image ({0})", error), false);
                await (new MessageDialog("Failed to save image. Try again.", "Sorry")).ShowAsync();

                return;
            }

            //save
            await AppData.CreateOrUpdateNote(viewModel.Note);
        }

        private void NoteImageContainer_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DeleteNoteImageMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            viewModel.TempNoteImage = (e.OriginalSource as FrameworkElement).DataContext as NoteImage;
            viewModel.DeleteNoteImageCommand.Execute(viewModel.TempNoteImage);
        }
    }
}
