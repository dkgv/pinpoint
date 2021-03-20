using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pinpoint.Core
{
    public class PluginSettings : ObservableCollection<PluginSetting>
    {
        private readonly Dictionary<string, PluginSetting> _settingsByName = new Dictionary<string, PluginSetting>();

        public void Put(string name, object value)
        {
            Add(_settingsByName[name] = new PluginSetting
            {
                Name = name,
                Value = value
            });
        }

        public object this[string name] => _settingsByName.GetValueOrDefault(name, null)?.Value;

        public bool Remove(string name)
        {
            if (!_settingsByName.ContainsKey(name))
            {
                return false;
            }

            Remove(_settingsByName[name]);
            return _settingsByName.Remove(name);
        }

        public string Str(string name) => this[name]?.ToString();

        public int Int(string name) => int.Parse(this[name]?.ToString() ?? "0");
    }
}