using System.Configuration;

namespace DirectorySizeChecker.Data
{
    public class UserSetting : ConfigurationElement
    {
        [ConfigurationProperty("userName", IsKey = true, IsRequired = true)]
        public string UserName
        {
            get
            {
                return (string)this["userName"];
            }
        }

        [ConfigurationProperty("maximalSizeMB", IsRequired = true)]
        public double MaximalSizeMB
        {
            get
            {
                return (double)this["maximalSizeMB"];
            }
        }
    }
}
