﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ProcessManager
{
    public class ProcessManagerPlugin: IPlugin
    {
        public PluginMeta Meta { get; set; } = new("Process Manager", PluginPriority.Highest);
        
        public PluginSettings UserSettings { get; set; } = new();
        
        public async Task<bool> Activate(Query query)
        {
            return query.RawQuery.Length >= 2 && query.Prefix(2).Equals("ps") && query.Parts.Length > 1;
        }

        public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var term = string.Join(' ', query.Parts[1..]).ToLower();
            var visited = new HashSet<string>();

            foreach (var process in System.Diagnostics.Process.GetProcesses().Where(p => visited.Add(p.ProcessName)))
            {
                var windowTitle = process.MainWindowTitle;
                var contains = windowTitle.ToLower().Contains(term);

                if (!contains)
                {
                    contains = process.ProcessName.ToLower().Contains(term);
                }

                var processName = $"{process.ProcessName}.exe";
                if (!contains)
                {
                    try
                    {
                        var description = FileVersionInfo.GetVersionInfo(process.MainModule?.FileName ?? string.Empty).FileDescription;
                        if (!string.IsNullOrEmpty(description))
                        {
                            processName = $"{description}";
                            contains = description.ToLower().Contains(term);
                        }
                    }
                    catch
                    {
                    }
                }

                if (contains)
                {
                    var subtitle = string.IsNullOrEmpty(windowTitle) ? $"{process.ProcessName}.exe" : windowTitle;
                    yield return new ProcessResult(process, processName, subtitle);
                }
            }
        }
    }

    public class ProcessResult: AbstractFontAwesomeQueryResult
    {
        private readonly Process _process;

        public ProcessResult(Process process, string processName, string subtitle): base($"Kill \"{processName}\" (PID {process.Id})", $"{subtitle}")
        {
            _process = process;
        }
        public override void OnSelect() => _process.Kill();

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_File;
    }
}
