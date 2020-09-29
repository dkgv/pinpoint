using System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace Pinpoint.Core
{
    public static class AppSettings
    {
        private static readonly Dictionary<object, object> Container = new Dictionary<object, object>();

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
            return Container.TryGetValue(key, out var value) ? value : null;
        }

        public static void Put(object key, object value)
        {
            Container.Add(key, value);
        }

        public static void Save()
        {
            var json = JsonSerializer.Serialize(Container);
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
    }
}
