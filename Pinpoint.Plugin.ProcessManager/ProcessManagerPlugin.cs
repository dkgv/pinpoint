using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ProcessManager
{
    public class ProcessManagerPlugin: IPlugin
    {
        public PluginMeta Meta { get; set; } = new PluginMeta("Process Manager", PluginPriority.Highest);
        public PluginSettings UserSettings { get; set; } = new PluginSettings();
        public void Unload()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Activate(Query query)
        {
            var shouldActivate = query.RawQuery.StartsWith("ps") && query.Parts.Length > 1;
            return Task.FromResult(shouldActivate);
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query)
        {
            var searchQuery = query.Parts[1].ToLower();
            var processes = System.Diagnostics.Process.GetProcesses();

            foreach (var process in processes)
            {
                if (process.ProcessName.ToLower().Contains(searchQuery) || (process.MainModule != null &&
                                                                            process.MainModule.FileVersionInfo
                                                                                .FileDescription.ToLower()
                                                                                .Contains(searchQuery)))
                {
                    yield return new ProcessResult(process);
                }
            }
        }
    }

    public class ProcessResult: AbstractFontAwesomeQueryResult
    {
        private readonly Process _process;

        public ProcessResult(Process process): base(process.MainModule?.FileVersionInfo.FileDescription ?? "none")
        {
            _process = process;
        }
        public override void OnSelect()
        {
            _process.Kill();
        }

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_File;
    }
}
