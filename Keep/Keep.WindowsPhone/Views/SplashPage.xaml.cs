using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Keep.Models;
using Keep.Utils;
using Keep.Views;

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
            App.ChangeStatusBarColor();

            //load notes
            AppData.Notes = await AppSettings.Instance.LoadNotes();

            //AppData.Notes = new Notes() {
            //    //new Note("Title", new Checklist() { new ChecklistItem("Item 1"), new ChecklistItem("Item 2"), new ChecklistItem("Item 3") }, NoteColor.ORANGE),
            //    new Note("Big text", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", NoteColor.BLUE),
            //    new Note("Big text", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.", NoteColor.GREEN),
            //    new Note("Title", "Hi", NoteColor.TEAL),
            //    new Note("Title", "Hi", NoteColor.DEFAULT),
            //    new Note("Title", "Hi", NoteColor.RED),
            //};

            NavigateAsync(typeof(MainPage));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
        }

        private async void NavigateAsync(Type sourcePageType, object parameter = null)
        {
            CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Task.Delay(0100);
                Frame.Navigate(sourcePageType, parameter);
            });
        }
    }
}