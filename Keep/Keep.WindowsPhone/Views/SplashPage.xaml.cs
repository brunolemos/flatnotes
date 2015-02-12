﻿using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Keep.Utils;
using Keep.Utils.Migration;
using Keep.Views;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Keep.Models;
using System.Linq;

namespace Keep
{
    public sealed partial class SplashPage : Page
    {
        object navigationParameter;

        public SplashPage()
        {
            InitializeComponent();

            Loaded += SplashPage_Loaded;
            SplashScreenImage.ImageOpened += (s, e) => { Window.Current.Activate(); };
            SplashScreenImage.ImageFailed += (s, e) => { Window.Current.Activate(); };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationParameter = e.Parameter;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            Frame.BackStack.Clear();
        }

        private async void NavigateAsync(Type sourcePageType, object parameter = null)
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Task.Delay(0300);

                Window.Current.Activate();
                Frame.Navigate(sourcePageType, parameter);
            });
        }

        private async void SplashPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            //versioning -- migrate app data structure when necessary
            await Migration.Migrate(AppSettings.Instance.Version);

            //load notes
            AppData.Notes = await AppSettings.Instance.LoadNotes();

            //load archived notes
            AppData.ArchivedNotes = await AppSettings.Instance.LoadArchivedNotes();

            NavigateAsync(typeof(MainPage), navigationParameter);
        }
    }
}