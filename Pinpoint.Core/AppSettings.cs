using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pinpoint.Core
{
    public static class AppSettings
    {
        private static readonly List<Setting> Settings = new List<Setting>();

        public static T GetAs<T>(object key)
        {
            var item = Get(key);
            return item == null ? default : (T)item;
        }

        public static string GetStr(object key)
        {
            return GetAs<string>(key);
        }

        public static object Get(object key)
        {
            return Settings.FirstOrDefault(s => s.Key.Equals(key));
        }

        public static void Put(object key, object value)
        {
            var index = IndexOf(key);

            if (index == -1)
            {
                Settings.Add(new Setting(key, value));
            }
            else
            {
                Settings[index] = new Setting(key, value);
            }
        }

        public static int IndexOf(object key)
        {
            return Settings.FindIndex(s => s.Key.Equals(key));
        }

        public static void Save()
        {
            if (!File.Exists(Constants.SettingsFilePath))
            {
                Directory.CreateDirectory(Constants.MainDirectory);

                File.Create(Constants.SettingsFilePath).Close();
            }

            var json = JsonSerializer.Serialize(Settings);
            File.WriteAllText(Constants.SettingsFilePath, json);
        }

        public static void Load()
        {
            var json = File.ReadAllText(Constants.SettingsFilePath);
            var container = JsonSerializer.Deserialize<Dictionary<object, object>>(json);

            foreach (var (key, value) in container)
            {
                Put(key, value);
            }
        }

        private class Setting
        {
            public Setting(object key, object value)
            {
                Key = key;
                Value = value;
            }

            public object Key { get; set; }

            public object Value { get; set; }
        }
    }
}
