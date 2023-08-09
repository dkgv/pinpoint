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
    public class ControlPanelPlugin : AbstractPlugin
    {
        private UkkonenTrie<ControlPanelItem> _controlPanelItems = new();
        private const string ControlPanelRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ControlPanel\NameSpace";

        public override PluginManifest Manifest { get; } = new("Control Panel Search")
        {
            Description = "Search for Windows control panel items."
        };

        public override Task<bool> Initialize()
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

            return Task.FromResult(true);
        }

        public void Unload()
        {
            _controlPanelItems = null;
        }

        public override async Task<bool> ShouldActivate(Query query)
        {
            return query.RawQuery.Length >= 3;
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            foreach (var item in _controlPanelItems.Retrieve(query.RawQuery.ToLower()).Where(i => i.RegistryKey != null))
            {
                yield return new ControlPanelResult(item.Name, item.Description, ControlPanelIconProvider.GetIcon(item.RegistryKey));
            }
        }

        private List<ControlPanelItem> LoadControlPanelItems()
        {
            const string args = "-NonInteractive -NoProfile -Command \"Get-ControlPanelItem\"";
            return ProcessHelper.PowerShellToJson<List<ControlPanelItem>>(args);
        }
    }
}
