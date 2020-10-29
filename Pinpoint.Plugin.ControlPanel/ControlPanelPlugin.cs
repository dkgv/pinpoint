using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.ControlPanel
{
    public class ControlPanelPlugin : IPlugin
    {
        private List<ControlPanelItem> _controlPanelItems;
        private const string ControlPanelRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ControlPanel\NameSpace";

        public PluginMeta Meta { get; set; } = new PluginMeta("Control Panel Search", PluginPriority.Standard);

        public void Load()
        {
            // Load actual control panel items
            _controlPanelItems = LoadControlPanelItems();

            // Lookup registry details for each item
            var registryRoot = Registry.LocalMachine.OpenSubKey(ControlPanelRegistryPath);
            foreach (var subKeyName in registryRoot?.GetSubKeyNames())
            {
                // Get default value
                var value = registryRoot.OpenSubKey(subKeyName)?.GetValue("");

                // Ensure registry entry actually is a control panel item
                var item = _controlPanelItems.FirstOrDefault(i => i.Name.ToLower().Equals(value?.ToString().ToLower()));
                if (item != null)
                {
                    item.RegistryKey = subKeyName;
                }
            }
        }

        public void Unload()
        {
            _controlPanelItems.Clear();
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 3;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            foreach (var item in _controlPanelItems.Where(i => i.Name.ToLower().Contains(query.RawQuery.ToLower()) && i.RegistryKey != null))
            {
                yield return new ControlPanelResult(item.Name, item.Description, ControlPanelIconProvider.GetIcon(item.RegistryKey));
            }
        }

        private List<ControlPanelItem> LoadControlPanelItems()
        {
            const string args = "-NonInteractive -NoProfile -Command \"Get-ControlPanelItem | ConvertTo-Json\"";

            var process = new Process
            {
                StartInfo =
                {
                    FileName = "powershell",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            var sb = new StringBuilder();
            while (!process.StandardOutput.EndOfStream)
            {
                sb.Append(process.StandardOutput.ReadLine());
            }

            return JsonConvert.DeserializeObject<List<ControlPanelItem>>(sb.ToString());
        }
    }
}
