using System;
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
        object parameter;

        public SplashPage()
        {
            this.InitializeComponent();

            this.Loaded += SplashPage_Loaded;
            this.SplashScreenImage.ImageOpened += (s, e) => { Window.Current.Activate(); };
            this.SplashScreenImage.ImageFailed += (s, e) => { Window.Current.Activate(); };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            parameter = e.Parameter;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            Frame.BackStack.Clear();
        }

        private async void NavigateAsync(Type sourcePageType, object parameter = null)
        {
            Window.Current.Activate();

            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Task.Delay(0200);
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

            //received a note via parameter (from secondary tile)
            if (parameter != null && !String.IsNullOrEmpty(parameter.ToString()))
            {
                var note = AppData.Notes.FirstOrDefault(n => n.ID == parameter.ToString());
                if (note == null) note = AppData.ArchivedNotes.FirstOrDefault(n => n.ID == parameter.ToString());

                if (note != null)
                {
                    NavigateAsync(typeof(NoteEditPage), note);
                    return;
                }
            }

            NavigateAsync(typeof(MainPage), parameter);
        }
    }
}