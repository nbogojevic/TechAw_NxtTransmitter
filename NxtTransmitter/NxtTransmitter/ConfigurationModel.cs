using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Windows;

namespace NxtTransmitter
{
    public class ConfigurationModel : INotifyPropertyChanged
    {
        IsolatedStorageSettings settings;

        // Declare the PropertyChanged event.
        public event PropertyChangedEventHandler PropertyChanged;


        public ConfigurationModel()
        {
            try
            {
                settings = IsolatedStorageSettings.ApplicationSettings;
            }
            catch (IsolatedStorageException e)
            {
                Debug.WriteLine(e.Message);
            }
   
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                valueChanged = true;
            }
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Save the settings.
        /// </summary>
        public void Save()
        {
            settings.Save();
        }

        public bool DebugMode
        {
            get
            {
                return GetValueOrDefault<bool>("DebugMode", false);
            }
            set
            {
                if (AddOrUpdateValue("DebugMode", value))
                {
                    Save();
                    NotifyPropertyChanged("DebugMode");
                    NotifyPropertyChanged("DebugPanelVisibility");
                }
            }
        }

        public bool ProductionMode
        {
            get
            {
                return GetValueOrDefault<bool>("ProductionMode", true);
            }
            set
            {
                if (AddOrUpdateValue("ProductionMode", value))
                {
                    Save();
                    NotifyPropertyChanged("ProductionMode");
                }
            }
        }
        public string DevelopmentUrl
        {
            get
            {
                return GetValueOrDefault<string>("DevelopmentUrl", "http://localhost:8080/winner");
            }
            set
            {
                if (AddOrUpdateValue("DevelopmentUrl", value))
                {
                    Save();
                    NotifyPropertyChanged("DevelopmentUrl");
                }
            }
        }
        public string ProductionUrl
        {
            get
            {
                return GetValueOrDefault<string>("ProductionUrl", "http://nxt-driver.appspot.com/winner");
            }
            set
            {
                if (AddOrUpdateValue("ProductionUrl", value))
                {
                    Save();
                    NotifyPropertyChanged("ProductionUrl");
                }
            }
        }
        public string PollingInterval
        {
            get
            {
                return GetValueOrDefault<string>("PollingInterval", "5");
            }
            set
            {
                if (AddOrUpdateValue("PollingInterval", value))
                {
                    Save();
                    NotifyPropertyChanged("PollingInterval");
                }
            }
        }
        public string TokenDuration
        {
            get
            {
                return GetValueOrDefault<string>("TokenDuration", "1");
            }
            set
            {
                if (AddOrUpdateValue("TokenDuration", value))
                {
                    Save();
                    NotifyPropertyChanged("TokenDuration");
                }
            }
        }



        // NotifyPropertyChanged will raise the PropertyChanged event, 
        // passing the source property that is being updated.
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }
        public bool DebugJson
        {
            get
            {
                return GetValueOrDefault<bool>("DebugJson", false);
            }
            set
            {
                if (AddOrUpdateValue("DebugJson", value))
                {
                    Save();
                    NotifyPropertyChanged("DebugJson");
                }
            }
        }
        public int PollingIntervalSeconds { get { return int.Parse(PollingInterval); } }
        public Visibility DebugPanelVisibility { get { return DebugMode ? Visibility.Visible : Visibility.Collapsed; } }
    }
}
