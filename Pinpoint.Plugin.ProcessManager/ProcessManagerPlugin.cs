using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FontAwesome5;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.ProcessManager
{
    public class ProcessManagerPlugin : AbstractPlugin
    {
        private List<Process> _cachedProcesses = new();
        private DateTime _lastProcessDump = DateTime.MinValue;

        public override PluginManifest Manifest { get; } = new("Process Manager", PluginPriority.High);

        public override async Task<bool> ShouldActivate(Query query)
        {
            return query.Raw.Length >= 2 && query.Prefix(2).Equals("ps") && query.Parts.Length > 1;
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var term = string.Join(' ', query.Parts[1..]).ToLower();
            foreach (var process in await FilterProcesses(term, ct))
            {
                yield return process;
            }
        }

        private async Task<List<ProcessResult>> FilterProcesses(string query, CancellationToken ct)
        {
            var results = new List<ProcessResult>();
            var all = await Task.Run(GetAllProcesses, ct);

            ProcessResult Collect(Process process)
            {
                var windowTitle = process.MainWindowTitle;
                var contains = windowTitle.ToLower().Contains(query);

                if (!contains)
                {
                    contains = process.ProcessName.ToLower().Contains(query);
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
                            contains = description.ToLower().Contains(query);
                        }
                    }
                    catch
                    {
                    }
                }

                if (!contains)
                {
                    return null;
                }

                var subtitle = string.IsNullOrEmpty(windowTitle) ? $"{process.ProcessName}.exe" : windowTitle;
                return new ProcessResult(process, processName, subtitle);
            }

            var tasks = new List<Task<ProcessResult>>();
            foreach (var process in all)
            {
                tasks.Add(Task.Run(() => Collect(process), ct));
            }

            var taskResults = await Task.WhenAll(tasks);
            results.AddRange(taskResults.Where(x => x != null));

            return results;
        }

        private List<Process> GetAllProcesses()
        {
            if (DateTime.Now - _lastProcessDump < TimeSpan.FromSeconds(10))
            {
                return _cachedProcesses;
            }

            // Old results expired
            var visited = new HashSet<string>();
            var start = DateTime.Now;
            var processes = Process.GetProcesses().Where(p => p.SessionId != 0 && visited.Add(p.ProcessName));

            _lastProcessDump = DateTime.Now;
            _cachedProcesses = processes.ToList();

            return _cachedProcesses;
        }
    }


    public class ProcessResult : AbstractFontAwesomeQueryResult
    {
        private readonly Process _process;

        public ProcessResult(Process process, string processName, string subtitle) : base($"Kill \"{processName}\" (PID {process.Id})", $"{subtitle}")
        {
            _process = process;
        }
        public override void OnSelect() => _process.Kill();

        public override EFontAwesomeIcon FontAwesomeIcon => EFontAwesomeIcon.Regular_File;
    }
}
