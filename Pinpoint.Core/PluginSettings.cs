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

        public object this[string name]
        {
            get => _settingsByName.GetValueOrDefault(name, null)?.Value;
            set => Put(name, value);
        }

        public bool Has(string name) => _settingsByName.ContainsKey(name);
        
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

        public int Int(string name) 
        {
            int.TryParse(this[name]?.ToString(), out var value);
            return value;
        }

        public void Overwrite(PluginSettings other)
        {
            foreach (var setting in other)
            {
                if (setting.Value != null)
                {
                    Put(setting.Name, setting.Value);
                }
            }
        }
    }
}