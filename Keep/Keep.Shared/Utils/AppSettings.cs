using System;
using System.Diagnostics;
using System.Collections.Generic;
//using Windows.Storage...

using Newtonsoft.Json;
using Keep.Models;

namespace Keep.Utils
{
    public class AppSettings
    {
        public static readonly AppSettings Instance = new AppSettings();
        //private IsolatedStorage settings;

        // The key names of our settings
        private const string LOGGEDUSER_KEY = "KeepSetting_LoggedUser";
        private const string TOURMESSAGES_KEY = "KeepSetting_TourMessages";

        // The default value of our settings
        private User LOGGEDUSER_DEFAULT = new User();
        private Dictionary<string, bool> TOURMESSAGES_DEFAULT = new Dictionary<string, bool>();

        private AppSettings()
        {
            // Get the settings for this application.
            //if ( !System.ComponentModel.DesignerProperties.IsInDesignTool )
            //    settings = IsolatedStorageSettings.ApplicationSettings;
        }

        public bool AddOrUpdateValue( string Key, Object value )
        {
            bool valueChanged = false;

            //// If the key exists
            //if ( settings.Contains( Key ) )
            //{
            //    // If the value has changed
            //    if ( settings[Key] != value )
            //    {
            //        valueChanged = true;

            //        // Store the new value
            //        settings[Key] = value;
            //    }

            //}
            //// Otherwise create the key.
            //else
            //{
            //    settings.Add( Key, value );
            //    valueChanged = true;
            //}

            return valueChanged;
        }

        public T GetValueOrDefault<T>( string Key, T defaultValue )
        {
            //// If the key exists, retrieve the value.
            //if ( settings.Contains( Key ) )
            //{
            //    return (T)settings[Key];
            //}

            // Otherwise, use the default value.
            return defaultValue;
        }

        public void Save()
        {
            //settings.Save();
        }

        public User LoggedUser
        {
            get { string json = GetValueOrDefault<string>( LOGGEDUSER_KEY, "" ); Debug.WriteLine( json ); return ( String.IsNullOrEmpty( json ) ) ? LOGGEDUSER_DEFAULT : JsonConvert.DeserializeObject<User>( json ); } // Debug.WriteLine("LoggedUser json: " + json);
            set { if ( AddOrUpdateValue( LOGGEDUSER_KEY, ( value == null ? "{}" : JsonConvert.SerializeObject( value ) ) ) ) Save(); }
        }

        public Dictionary<string, bool> TourMessages
        {
            get { string json = GetValueOrDefault<string>( TOURMESSAGES_KEY, "" ); return ( String.IsNullOrEmpty( json ) ) ? TOURMESSAGES_DEFAULT : JsonConvert.DeserializeObject<Dictionary<string, bool>>( json ); }
            set { if ( AddOrUpdateValue( TOURMESSAGES_KEY, JsonConvert.SerializeObject( value ) ) ) Save(); }
        }
    }
}