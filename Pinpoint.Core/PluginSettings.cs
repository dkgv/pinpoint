using System.Collections.ObjectModel;
using System.Linq;

namespace Pinpoint.Core
{
    public class PluginSettings : ObservableCollection<PluginSetting>
    {
        public void Put(string name, object value)
        {
            var setting = new PluginSetting
            {
                Name = name,
                Value = value
            };
            Add(setting);
        }

        public bool Bool(string key)
        {
            var setting = Str(key).ToLower();
            return setting.Equals("yes") || setting.Equals("true");
        }

        public string Str(string key) => Get(key).Value.ToString();

        private PluginSetting Get(string key) => this.FirstOrDefault(setting => setting.Name.Equals(key));
    }
}