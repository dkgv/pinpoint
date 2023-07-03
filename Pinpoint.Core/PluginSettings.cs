using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pinpoint.Core;

public class PluginStorage
{
    public PluginStorage()
    {
        UserSettings = new PluginSettings();
        InternalSettings = new Dictionary<string, object>();
    }

    public PluginSettings UserSettings { get; set; }

    public Dictionary<string, object> InternalSettings { get; set; }
}

public class PluginSettings : ObservableCollection<StorageEntry>
{
    public bool Contains(string key) => this.Any(entry => entry.Name == key);

    public void Put(string name, object value) => Add(new StorageEntry(name, value));

    public bool Bool(string key)
    {
        var setting = Str(key).ToLower();
        return setting.Equals("yes") || setting.Equals("true");
    }

    public string Str(string key) => Get(key).Value.ToString();

    private StorageEntry Get(string key) => this.FirstOrDefault(setting => setting.Name.Equals(key));
}

public record StorageEntry(string Name, object Value);
