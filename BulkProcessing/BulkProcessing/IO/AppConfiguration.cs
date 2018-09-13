using System.Configuration;

namespace BulkProcessing.IO
{
    public class AppConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("mainDirectoryPath", IsRequired = true)]
        public string MainDirectoryPath
        {
            get
            {
                return (string)this["mainDirectoryPath"];
            }
        }

        [ConfigurationProperty("modifiedDateMinutes", IsRequired = true)]
        public double ModifiedDateMinutes
        {
            get
            {
                return (double)this["modifiedDateMinutes"];
            }
        }

        [ConfigurationProperty("geoServerWPSUrl", IsRequired = true)]
        public string GeoServerWPSUrl
        {
            get
            {
                return (string)this["geoServerWPSUrl"];
            }
        }
    }
}
