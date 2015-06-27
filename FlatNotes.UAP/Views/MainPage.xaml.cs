using FlatNotes.Common;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FlatNotes.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = new MainViewModel();

        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public ObservableCollection<NavLink> NavLinks { get; } = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "Notes", Symbol = Symbol.Emoji  },
            new NavLink() { Label = "Reminders", Symbol = Symbol.Clock },
            new NavLink() { Label = "Archive", Symbol = Symbol.Emoji },
            new NavLink() { Label = "Trash", Symbol = Symbol.Delete },
        };

        public ObservableCollection<NavLink> NavFooterLinks { get; } = new ObservableCollection<NavLink>()
        {
            new NavLink() { Label = "Accounts", Symbol = Symbol.Contact },
            new NavLink() { Label = "Feedback", Symbol = Symbol.Favorite },
            new NavLink() { Label = "Settings", Symbol = Symbol.Setting, TargetPageType = typeof(SettingsPage) },
        };

        private void OpenSettingsPage()
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        public MainPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            Loaded += (s, e) => { App.ResetStatusBar(); };
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }


        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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

        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var navLink = e.ClickedItem as NavLink;
            if (navLink?.TargetPageType == null) return;

            MainSplitView.IsPaneOpen = false;
            Frame.Navigate(navLink.TargetPageType);
        }

        private async void NotesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Note note = e.ClickedItem as Note;
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

        private void MenuToggleButton_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        //private void NotesListView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        //{
        //    var listControl = sender as ItemsControl;
        //    if (listControl.ItemsPanelRoot?.Children == null) return;

        //    UpdateListAppearence(listControl.ItemsPanelRoot?.Children);
        //}

        //private void UpdateListAppearence(UIElementCollection elements)
        //{
        //    if (elements == null || elements.Count <= 0) return;

        //    int columns = 4, nextColumn = 0;
        //    double[] lastYInColumn = new double[columns];
        //    UIElement[] lastElementInColumn = new UIElement[columns];

        //    for (int i = 0; i < elements.Count; i++)
        //    {
        //        var item = elements[i];

        //        if (i < columns)
        //        {
        //            lastElementInColumn[i] = item;
        //            lastYInColumn[i] = item.DesiredSize.Height;

        //            if (i > 0) RelativePanel.SetRightOf(item, elements[i - 1]);

        //            continue;
        //        }

        //        for (int j = columns - 1; j >= 0; j--)
        //            if (lastYInColumn[j] <= lastYInColumn[nextColumn])
        //                nextColumn = j;

        //        RelativePanel.SetBelow(item, lastElementInColumn[nextColumn]);
        //        RelativePanel.SetAlignHorizontalCenterWith(item, lastElementInColumn[nextColumn]);

        //        lastElementInColumn[nextColumn] = item;
        //        lastYInColumn[nextColumn] += item.DesiredSize.Height;
        //    }
        //}
    }
}
