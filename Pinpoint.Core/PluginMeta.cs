using System;
using System.Collections.Generic;

namespace Pinpoint.Core;

public class PluginManifest
{
    public PluginManifest(string name) : this(name, PluginPriority.Standard)
    {
    }

    public PluginManifest(string name, PluginPriority priority)
    {
        Name = name;
        Priority = priority;
    }

    public string Name { get; set; }

    public PluginPriority Priority { get; set; } = PluginPriority.Standard;

    public string Description { get; set; }
}

public enum PluginPriority
{
    High = int.MaxValue,
    Standard = 0,
    Low = int.MinValue
}

public class PluginState
{
    public bool IsEnabled { get; set; } = false;

    public bool HasModifiableSettings { get; set; } = false;

    public TimeSpan DebounceTime { get; set; } = TimeSpan.Zero;
}

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

public class UserSettings : Dictionary<string, object>
{
    public bool Bool(string key)
    {
        var setting = Str(key).ToLower();
        return setting.Equals("yes") || setting.Equals("true");
    }

    public string Str(string key) => this[key].ToString();
}
