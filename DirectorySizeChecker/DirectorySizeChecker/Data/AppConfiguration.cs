using System;
using System.Configuration;

namespace DirectorySizeChecker.Data
{
    public class AppConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("mainDirectoryPath", IsRequired = true)]
        public string MainDirectoryPath
        {
            get
            {
                return (String)this["mainDirectoryPath"];
            }
        }

        [ConfigurationProperty("defaultMaximalSizeMB", IsRequired = true)]
        public double DefaultMaximalSizeMB
        {
            get
            {
                return (double)this["defaultMaximalSizeMB"];
            }
        }

        [ConfigurationProperty("", IsRequired = false, IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(UserSettingCollection), AddItemName = "result")]
        public UserSettingCollection UserSettings
        {
            get
            {
                return (UserSettingCollection)this[""];
            }
        }
    }
}
