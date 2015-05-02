using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace FlatNotes.Converters
{
    public sealed class LocalizedResourcesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null) return "";

            try
            {
                ResourceLoader resourceLoader = new ResourceLoader();
                return resourceLoader.GetString((string)parameter);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
