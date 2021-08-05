using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gma.DataStructures.StringSearch;
using Microsoft.Win32;
using Newtonsoft.Json;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ControlPanel
{
    public class ControlPanelPlugin : IPlugin
    {
        private const string Description = "Search for Windows control panel items.";

        private UkkonenTrie<ControlPanelItem> _controlPanelItems = new UkkonenTrie<ControlPanelItem>();
        private const string ControlPanelRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ControlPanel\NameSpace";

        public PluginMeta Meta { get; set; } = new PluginMeta("Control Panel Search", Description, PluginPriority.Standard);

        public PluginSettings UserSettings { get; set; } = new PluginSettings();

        public bool IsLoaded { get; set; }

        public Task<bool> TryLoad()
        {
            // Load actual control panel items
            var items = LoadControlPanelItems();

            // Lookup registry details for each item
            var registryRoot = Registry.LocalMachine.OpenSubKey(ControlPanelRegistryPath);
            foreach (var subKeyName in registryRoot?.GetSubKeyNames())
            {
                // Get default value
                var value = registryRoot.OpenSubKey(subKeyName)?.GetValue("");

                // Ensure registry entry actually is a control panel item
                var item = items.FirstOrDefault(i => i.Name.ToLower().Equals(value?.ToString().ToLower()));
                if (item != null)
                {
                    item.RegistryKey = subKeyName;
                }
            }

            foreach (var controlPanelItem in items)
            {
                _controlPanelItems.Add(controlPanelItem.Name.ToLower(), controlPanelItem);
            }

            IsLoaded = true;
            return Task.FromResult(IsLoaded);
        }

        public void Unload()
        {
            _controlPanelItems = null;
        }

        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 3;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var item in _controlPanelItems.Retrieve(query.RawQuery.ToLower()).Where(i => i.RegistryKey != null))
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
