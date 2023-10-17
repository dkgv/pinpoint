using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Plugin.AppSearch.Models;

namespace Pinpoint.Plugin.AppSearch.Providers;

public class DirectoryAppProvider : IAppProvider
{
    private readonly string[] _paths;
    private readonly Dictionary<string, DateTime> _lastPathWriteCache = new();
    private readonly List<IApp> _cachedApps = new();
    private readonly HashSet<string> _uniqueAppNames = new();

    public DirectoryAppProvider(string[] paths)
    {
        _paths = paths;
    }

    public async Task<IEnumerable<IApp>> Provide()
    {
        await foreach (var path in GetPathsNeedUpdate())
        {
            var shortcuts = await FindApps(path);
            foreach (var shortcut in shortcuts)
            {
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(shortcut);
                if (!_uniqueAppNames.Add(nameWithoutExtension))
                {
                    continue;
                }

                _cachedApps.Add(new StandardApp
                {
                    Name = nameWithoutExtension,
                    FilePath = shortcut,
                    IconLocation = shortcut
                });
            }
        }

        var res = new List<IApp>();
        for (int i = 0; i < _cachedApps.Count;)
        {
            var app = _cachedApps[i];
            if (!File.Exists(app.FilePath))
            {
                _uniqueAppNames.Remove(app.Name);
                _cachedApps.Remove(app);
                _lastPathWriteCache[Path.GetDirectoryName(app.FilePath)] = DateTime.MinValue;
                continue;
            }

            i++;
            res.Add(app);
        }

        return res;
    }

    private async Task<IEnumerable<string>> FindApps(string path)
    {
        var extensions = new[]
        {
            "*.appref-ms",
            "*.lnk",
            "*.exe"
        };

        async Task<IEnumerable<string>> SafeGetFiles(string ext)
        {
            try
            {
                return await Task.Run(() => Directory.GetFiles(path, ext, SearchOption.AllDirectories));
            }
            catch (UnauthorizedAccessException)
            {
                return Array.Empty<string>();
            }
        }

        return await Task.WhenAll(extensions.Select(SafeGetFiles)).ContinueWith(t => t.Result.SelectMany(x => x));
    }

    private async IAsyncEnumerable<string> GetPathsNeedUpdate()
    {
        foreach (var path in _paths.Where(Directory.Exists))
        {
            var currentLastWrite = await Task.Run(() => Directory.GetLastWriteTime(path));
            if (_lastPathWriteCache.ContainsKey(path) && currentLastWrite <= _lastPathWriteCache[path])
            {
                continue;
            }

            _lastPathWriteCache[path] = currentLastWrite;
            yield return path;
        }
    }
}