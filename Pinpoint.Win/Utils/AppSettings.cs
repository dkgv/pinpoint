using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pinpoint.Win.Utils;

public static class AppSettings
{
    private static readonly string UserHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string MainDirectory = UserHome + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;
    private static readonly string SettingsFilePath = MainDirectory + "Settings.json";

    private static readonly Dictionary<string, object> Settings = new();

    public static T Get<T>(string key) where T : class
    {
        TryGet(key, out T settings);
        return settings;
    }

    public static T GetOrDefault<T>(string key, T defaultValue) where T : class
    {
        TryGet(key, out T settings);

        return settings ?? defaultValue;
    }

    public static bool TryGet<T>(string key, out T settings) where T : class
    {
        settings = null;

        if (!Settings.TryGetValue(key, out var value) || value?.ToString() == null)
        {
            return false;
        }

        try
        {
            // Deserialize the object from the string value. or a string from a string value.
            settings = typeof(T) == typeof(string) ? value.ToString() as T : JsonConvert.DeserializeObject<T>(value.ToString());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static void Put(string key, object value, bool overwrite = true)
    {
        JToken token;
        if (value.GetType().IsArray)
        {
            token = JArray.FromObject(value);
        }
        else if (value is string)
        {
            token = value.ToString();
        }
        else
        {
            token = JObject.FromObject(value);
        }

        if (!Contains(key) || overwrite)
        {
            Settings[key] = token;
        }
    }

    public static void PutAndSave(string key, object value)
    {
        Put(key, value);
        Save();
    }

    public static bool Contains(string key) => Settings.ContainsKey(key);

    public static void Save()
    {
        if (!File.Exists(SettingsFilePath))
        {
            Directory.CreateDirectory(MainDirectory);
            File.Create(SettingsFilePath).Close();
        }

        var pairs = new Dictionary<string, object>();
        foreach (var setting in Settings)
        {
            pairs[setting.Key] = setting.Value;
        }

        File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(pairs));
    }

    public static bool Load()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return false;
        }

        var json = File.ReadAllText(SettingsFilePath);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        var container = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        foreach (var setting in container)
        {
            PutInitial(setting.Key, setting.Value);
        }

        return true;
    }

    private static void PutInitial(string key, object value, bool overwrite = true)
    {
        if (!Contains(key) || overwrite)
        {
            Settings[key] = value;
        }
    }
}
