using System;
using Windows.Foundation;

namespace FlatNotes.Utils.Migration.Versions.v1.Controllers
{
    public class DeviceController
    {
        private DeviceController() {}

        public static Version OSVersion { get { return null/*Environment.OSVersion.Version*/; } }

        public static string UserAnonymousID
        {
            get
            {
                if (System.Diagnostics.Debugger.IsAttached) return "DLecWIzro+qaLBkkx1vUx2ZZx5r3RZ8mCbS3SZKhnCY=";


                /*object obj;
                if (UserExtendedProperties.TryGetValue("ANID2", out obj))
                    if( obj != null && !String.IsNullOrEmpty(obj.ToString()) )
                        return obj.ToString();

                if (UserExtendedProperties.TryGetValue("ANID", out obj))
                {
                    if (obj != null)
                    {
                        string anid = obj.ToString();

                        string[] parts = anid.Split('&');
                        IEnumerable<string[]> pairs = parts.Select(part => part.Split('='));

                        string id = pairs
                            .Where(pair => pair.Length == 2 && pair[0] == "A")
                            .Select(pair => pair[1])
                            .FirstOrDefault();

                        return id;
                    }
                }*/

                return null;
            }
        }

        public static string DeviceID { get { return Convert.ToBase64String( GetDeviceExtendedPropertyOrDefault<byte[]>( "DeviceUniqueId" ) ); } }
        public static Size PhysicalScreenResolution { get { return GetDeviceExtendedPropertyOrDefault<Size>( "PhysicalScreenResolution" ); } }
        public static long ApplicationCurrentMemoryUsage { get { return GetDeviceExtendedPropertyOrDefault<long>( "ApplicationCurrentMemoryUsage" ); } }
        public static long ApplicationPeakMemoryUsage { get { return GetDeviceExtendedPropertyOrDefault<long>( "ApplicationPeakMemoryUsage" ); } }

        protected static T GetDeviceExtendedPropertyOrDefault<T>( string propertyName )
        {
            /*object obj;
            if ( DeviceExtendedProperties.TryGetValue( propertyName, out obj ) )
                return (T)obj;*/

            return default(T);
        }
    }
}
