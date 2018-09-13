using System.Collections.Generic;
using System.Configuration;

namespace DirectorySizeChecker.Data
{
    public class UserSettingCollection : ConfigurationElementCollection, IEnumerable<UserSetting>
    {
        private readonly List<UserSetting> userSettings;

        public UserSettingCollection()
        {
            userSettings = new List<UserSetting>();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            UserSetting userSetting = new UserSetting();
            userSettings.Add(userSetting);

            return userSetting;
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((UserSetting)element).UserName;
        }

        public new IEnumerator<UserSetting> GetEnumerator()
        {
            return userSettings.GetEnumerator();
        }
    }
}
