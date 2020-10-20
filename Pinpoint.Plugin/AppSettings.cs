using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pinpoint.Plugin
{
    public static class AppSettings
    {
        private static readonly Dictionary<string, object> Settings = new Dictionary<string, object>();

        public static T GetAs<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(Get(key).ToString());
        }

        public static List<T> GetListAs<T>(string key)
        {
            if (!Contains(key))
            {
                return new List<T>();
            }
            var json = GetAs<JArray>(key);
            return json.AsEnumerable()
                .Select(elem => JsonConvert.DeserializeObject<T>(elem.ToString()))
                .ToList();
        }

        public static string GetStrOrDefault(string key, string fallback)
        {
            return !Contains(key) ? fallback : GetStr(key);
        }

        public static T GetAsOrDefault<T>(string key, T fallback)
        {
            return !Contains(key) ? fallback : GetAs<T>(key);
        }

        public static string GetStr(string key)
        {
            return GetAs<string>(key);
        }

        public static object Get(string key)
        {
            return Settings[key];
        }

        public static void Put(string key, object value, bool overwrite = true)
        {
            if (!Contains(key) || overwrite)
            {
                Settings[key] = value;
            }
        }

        public static void PutAndSave(string key, object value)
        {
            Put(key, value);
            Save();
        }

        public static bool Contains(string key)
        {
            return Settings.ContainsKey(key);
        }

        public static void Save()
        {
            if (!File.Exists(AppConstants.SettingsFilePath))
            {
                Directory.CreateDirectory(AppConstants.MainDirectory);
                File.Create(AppConstants.SettingsFilePath).Close();
            }

            var pairs = new Dictionary<string, object>();
            foreach (var setting in Settings)
            {
                pairs[setting.Key] = setting.Value;
            }
            File.WriteAllText(AppConstants.SettingsFilePath, JsonConvert.SerializeObject(pairs));
        }

        public static bool Load()
        {
            if (!File.Exists(AppConstants.SettingsFilePath))
            {
                return false;
            }

            var json = File.ReadAllText(AppConstants.SettingsFilePath);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var container = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            foreach (var setting in container)
            {
                Put(setting.Key, setting.Value);
            }

            return true;
        }
    }
}
