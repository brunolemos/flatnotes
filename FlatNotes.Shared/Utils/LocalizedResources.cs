using Windows.ApplicationModel.Resources;

namespace FlatNotes.Utils
{
    public class LocalizedResources
    {
        public static LocalizedResources Instance { get { return instance; } }
        private static LocalizedResources instance = new LocalizedResources();

        private ResourceLoader resourceLoader = new ResourceLoader();

        public string Reminder { get { return resourceLoader.GetString("ReminderAppBarButton.Label"); } }
        public string ArchivedAtFormatString { get { return resourceLoader.GetString("ArchivedAtFormatString"); } }
        public string UpdatedAtFormatString { get { return resourceLoader.GetString("UpdatedAtFormatString"); } }
        public string CreatedAtFormatString { get { return resourceLoader.GetString("CreatedAtFormatString"); } }
    }
}