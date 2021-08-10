using System.Collections.ObjectModel;

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

        public string Str(string name)
        {
            foreach (var setting in this)
            {
                if (setting.Name.Equals(name))
                {
                    return setting.Value.ToString();
                }
            }
            return null;
        }
    }
}