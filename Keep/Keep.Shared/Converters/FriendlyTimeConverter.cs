using System;
using System.Diagnostics;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace Keep.Converters
{
    public class FriendlyTimeConverter : IValueConverter
    {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            DateTime receivedDateTime = DateTime.Parse(value.ToString());
            DateTime currentDateTime = DateTime.UtcNow;

            int delta = (int)currentDateTime.Subtract(receivedDateTime).TotalSeconds;
            TimeSpan ts_delta = new TimeSpan(0, 0, delta);

            //Debug.WriteLine("receivedDateTime:" + receivedDateTime);
            //Debug.WriteLine("currentDateTime:" + currentDateTime);
            //Debug.WriteLine("delta:" + delta);

            if (delta < 0) return "";
            if (delta < 30 * SECOND) return GetGlobalizedString("JustNow");
            if (delta < 2 * MINUTE) return GetGlobalizedString("AMinuteAgo");
            if (delta < 45 * MINUTE) return String.Format(GetGlobalizedString("NMinutesAgo"), ts_delta.Minutes);
            if (delta < 120 * MINUTE) return GetGlobalizedString("AnHourAgo");
            if (delta < 24 * HOUR) return String.Format(GetGlobalizedString("NHoursAgo"), ts_delta.Hours);
            if (delta < 48 * HOUR) return GetGlobalizedString("Yesterday");
            if (delta < 7 * DAY) return String.Format(GetGlobalizedString("NDaysAgo"), ts_delta.Days);
            if (delta < 14 * DAY) return GetGlobalizedString("AWeekAgo");
            if (delta < 30 * DAY) return String.Format(GetGlobalizedString("NWeeksAgo"), Math.Round((double)(ts_delta.Days / 7)));
            if (delta < 12 * MONTH)
            {
                int months = System.Convert.ToInt32(Math.Floor((double)ts_delta.Days / 30));
                return months <= 1 ? GetGlobalizedString("AMonthAgo") : String.Format(GetGlobalizedString("NMonthsAgo"), months);
            }
            else
            {
                int years = System.Convert.ToInt32(Math.Floor((double)ts_delta.Days / 365));
                return years <= 1 ? GetGlobalizedString("AYearAgo") : String.Format(GetGlobalizedString("NYearsAgo"), years);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value == null ? "" : value.ToString();
        }

        protected DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTimeStamp).ToLocalTime();
        }

        protected string GetGlobalizedString(string resourceKey)
        {
            return ResourceLoader.GetForCurrentView().GetString("PrettyDate_" + resourceKey);
        }

    }
}