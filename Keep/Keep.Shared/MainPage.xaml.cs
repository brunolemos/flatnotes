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

using Keep.Commands;
using Keep.Controls;

namespace Keep
{
    public sealed partial class MainPage : Page
    {
        private SendMessageCommand sendMessageCommand = new SendMessageCommand();
        public SendMessageCommand SendMessageCommand { get { return sendMessageCommand; } }

        public MainPage()
        {
            this.InitializeComponent();

#if WINDOWS_PHONE_APP
            StatusBar statusBar = StatusBar.GetForCurrentView();
            statusBar.BackgroundOpacity = 1;
            statusBar.BackgroundColor = ((SolidColorBrush)(App.Current.Resources["KeepBrandBackgroundBrush"])).Color;
            statusBar.ForegroundColor = ((SolidColorBrush)(App.Current.Resources["KeepBrandForegroundBrush"])).Color;

            //ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
#endif

            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.DataContext = this;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            this.Background = (SolidColorBrush)App.Current.Resources["KeepBackgroundBrush"];
        }

        private void GridView_Holding(object sender, HoldingRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            (sender as ListView).ReorderMode = ListViewReorderMode.Enabled;
#endif
        }

        private void ListView_Tapped(object sender, TappedRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (e.OriginalSource is Grid) {
                (sender as ListView).ReorderMode = ListViewReorderMode.Disabled;
            }
#endif
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
    }
}
