using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Keep.Controls;
using Keep.ViewModels;

namespace Keep
{
    public sealed partial class MainPage : Page
    {
        private MainPageViewModel viewModel = new MainPageViewModel();

        public MainPage()
        {
            this.InitializeComponent();

//#if WINDOWS_PHONE_APP
//            StatusBar statusBar = StatusBar.GetForCurrentView();
//            statusBar.BackgroundOpacity = 1;
//            statusBar.BackgroundColor = ((SolidColorBrush)(App.Current.Resources["KeepStatusBarBackgroundBrush"])).Color;
//            statusBar.ForegroundColor = ((SolidColorBrush)(App.Current.Resources["KeepStatusBarForegroundBrush"])).Color;

//            //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
//#endif

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //this.Background = (SolidColorBrush)App.Current.Resources["ApplicationPageBackgroundThemeBrush"];

            //view model - data context
            this.DataContext = viewModel;
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            int columns = 2;
            var listView = sender as ListView;
            var listViewPanel = listView.ItemsPanelRoot as ItemsWrapGrid;

            listViewPanel.ItemWidth = listView.ActualWidth / columns;

            //if (listViewPanel.Children.Count < 1) return;
            //double height = listViewPanel.Children[0].DesiredSize.Height;

            //int i = 0;
            //foreach (var child in listViewPanel.Children)
            //{
            //    i++;

            //    //...
            //}
        }

        private void GridView_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            (sender as ListView).ReorderMode = ListViewReorderMode.Enabled;
#endif
        }

        private void NotesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if ((sender as ListView).ReorderMode == ListViewReorderMode.Enabled)
                return;
#endif

            object parameter = e.ClickedItem;
            Frame.Navigate(typeof(NoteEditPage), parameter);
        }
    }
}
