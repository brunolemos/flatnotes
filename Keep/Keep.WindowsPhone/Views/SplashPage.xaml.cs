using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Keep.Models;
using Keep.Utils;
using Keep.Utils.Migration;
using Keep.Views;
using Windows.UI.ViewManagement;
using Windows.Storage;
using System.Diagnostics;

namespace Keep
{
    public sealed partial class SplashPage : Page
    {
        public SplashPage()
        {
            this.InitializeComponent();

            this.Loaded += SplashPage_Loaded;
        }

        private async void SplashPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            //versioning -- migrate app data structure when necessary
            await Migration.Migrate(AppSettings.Instance.Version);

            //load notes

            Task.Run(async () => {
                AppData.Notes = await AppSettings.Instance.LoadNotes();
            }).Wait();

            NavigateAsync(typeof(MainPage));

            //load archived notes
            AppData.ArchivedNotes = await AppSettings.Instance.LoadArchivedNotes();
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
                await Task.Delay(0200);
                Frame.Navigate(sourcePageType, parameter);
            });
        }
    }
}