using Keep.Common;
using Keep.Models;
using Keep.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Keep.Views
{
    public sealed partial class MainPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public MainViewModel viewModel { get { return (MainViewModel)DataContext; } }

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += MainPage_Loaded;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            App.Watch.Stop();
            LoadedEllapsedTime.Text = String.Format("content shown after {0}ms", App.Watch.ElapsedMilliseconds.ToString());
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            LoadStateEllapsedTime.Text = String.Format("navigated in {0}ms", App.Watch.ElapsedMilliseconds.ToString());
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

        private void NotesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            App.Watch.Restart();
            Frame.Navigate(typeof(NoteEditPage), e.ClickedItem);
        }
    }
}
