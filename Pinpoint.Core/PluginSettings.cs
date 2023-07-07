using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pinpoint.Core;

public class PluginStorage
{
    public PluginStorage()
    {
        User = new UserSettings();
        Internal = new Dictionary<string, object>();
    }

    public UserSettings User { get; set; }

    public Dictionary<string, object> Internal { get; set; }
}

public class UserSettings : ObservableCollection<Record>
{
    public bool Contains(string key) => this.Any(entry => entry.Name == key);

    public void Put(string name, object value) => Add(new Record(name, value));

    public bool Bool(string key)
    {
        var setting = Str(key).ToLower();
        return setting.Equals("yes") || setting.Equals("true");
    }

    public string Str(string key) => Get(key).Value.ToString();

    private Record Get(string key) => this.FirstOrDefault(setting => setting.Name.Equals(key));

    public static UserSettings Default(Dictionary<string, object> settings)
    {
        var instance = new UserSettings();
        foreach (var setting in settings)
        {
            if (!instance.Contains(setting.Key))
            {
                instance.Put(setting.Key, setting.Value);
            }
        }
        return instance;
    }
}

public record Record(string Name, object Value);
