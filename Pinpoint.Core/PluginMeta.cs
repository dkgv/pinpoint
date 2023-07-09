﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
    public PluginState()
    {
    }

    public bool? IsEnabled { get; set; }

    public bool? HasModifiableSettings { get; set; } = false;

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

public class UserSettings : ObservableCollection<MutableKeyValuePair<string, object>>
{
    public void Add(string key, object value)
    {
        Add(new MutableKeyValuePair<string, object>(key, value));
    }

    public object this[string key]
    {
        get
        {
            var setting = this.FirstOrDefault(s => s.Key.Equals(key));
            return setting.Value;
        }
        set
        {
            var setting = this.FirstOrDefault(s => s.Key.Equals(key));
            if (!string.IsNullOrEmpty(setting.Key))
            {
                var index = IndexOf(setting);
                this[index] = new MutableKeyValuePair<string, object>(key, value);
            }
            else
            {
                Add(new MutableKeyValuePair<string, object>(key, value));
            }
        }
    }

    public bool Bool(string key)
    {
        var setting = Str(key).ToLower();
        return setting.Equals("yes") || setting.Equals("true");
    }

    public string Str(string key) => this[key].ToString();
}

public class MutableKeyValuePair<TKey, TValue>
{
    public MutableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    public TKey Key { get; set; }

    public TValue Value { get; set; }
}
