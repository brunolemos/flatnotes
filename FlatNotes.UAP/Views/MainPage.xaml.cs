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

namespace FlatNotes.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel viewModel { get { return _viewModel; } }
        private static MainViewModel _viewModel = new MainViewModel();

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
            new NavLink() { Label = "Settings", Symbol = Symbol.Setting },
        };

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += (s, e) => { App.ResetStatusBar(); };
        }

        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {
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
