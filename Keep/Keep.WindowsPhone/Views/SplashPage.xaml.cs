using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Keep.Utils;
using Keep.Utils.Migration;
using Keep.Views;
using Windows.UI.ViewManagement;

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

            //simulation
            //Task.Run(async () =>
            //{
            //    await Windows.Storage.ApplicationData.Current.SetVersionAsync(0, (x) => { });

            //    string json = "{\"AnonymousID\":null,\"DevicesList\":[],\"Name\":null,\"Email\":null,\"Notes\":[{\"ID\":\"a\",\"IsChecklist\":false,\"Title\":\"A\",\"Text\":\"A\",\"Images\":[],\"Checklist\":null,\"CreatedAt\":\"2015-01-18T19:22:40.0986598-02:00\",\"UpdatedAt\":\"2015-01-18T19:22:40.0986598-02:00\",\"Color\":\"BLUE\"},{\"ID\":\"a\",\"IsChecklist\":true,\"Title\":\"C\",\"Text\":\"\",\"Images\":[],\"Checklist\":[{\"Text\":\"C\",\"IsChecked\":false},{\"Text\":\"C\",\"IsChecked\":false},{\"Text\":\"C\",\"IsChecked\":true}],\"CreatedAt\":\"2015-01-18T19:23:05.3232166-02:00\",\"UpdatedAt\":\"2015-01-18T19:23:05.3232166-02:00\",\"Color\":\"GREEN\"}],\"Preferences\":{\"Columns\":3,\"ItemMinWidth\":150.0,\"Theme\":0},\"CreatedAt\":\"2015-01-18T19:06:32.6837239-02:00\",\"UpdatedAt\":\"0001-01-01T00:00:00\",\"LastSeenAt\":\"2015-01-18T19:06:32.6867434-02:00\"}"; //,\"ArchivedNotes\":[{\"ID\":\"63557205769921_32577953\",\"IsChecklist\":true,\"Title\":\"B\",\"Text\":\"\",\"Images\":[],\"Checklist\":[{\"Text\":\"B\",\"IsChecked\":false},{\"Text\":\"B\",\"IsChecked\":true},{\"Text\":\"B\",\"IsChecked\":false}],\"CreatedAt\":\"2015-01-18T19:22:49.9209317-02:00\",\"UpdatedAt\":\"2015-01-18T19:22:49.9209317-02:00\",\"Color\":\"YELLOW\"},{\"ID\":\"63557205760085_92979168\",\"IsChecklist\":false,\"Title\":\"A\",\"Text\":\"A\",\"Images\":[],\"Checklist\":[],\"CreatedAt\":\"2015-01-18T19:22:40.0986598-02:00\",\"UpdatedAt\":\"2015-01-18T19:22:40.0986598-02:00\",\"Color\":\"BLUE\"}]
            //    await Utils.Migration.Versions.v1.AppSettings.Instance.AddOrUpdateValue("KeepSetting_LoggedUser", json);

            //    Utils.Migration.Versions.v1.AppSettings.Instance.LoggedUser = Newtonsoft.Json.JsonConvert.DeserializeObject<Utils.Migration.Versions.v1.Models.User>(json);
            //}).Wait();

            //versioning -- migrate app data structure when necessary
            await Migration.Migrate(AppSettings.Instance.Version);

            //load notes
            AppData.Notes = await AppSettings.Instance.LoadNotes();

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