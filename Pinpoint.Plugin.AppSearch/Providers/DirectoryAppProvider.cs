using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pinpoint.Plugin.AppSearch.Providers
{
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

        public IEnumerable<IApp> Provide()
        {
            foreach (var path in GetPathsNeedUpdate())
            {
                foreach (var shortcut in FindApps(path))
                {
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(shortcut);
                    if (!_uniqueAppNames.Add(nameWithoutExtension))
                    {
                        continue;
                    }

                    var app = new StandardApp
                    {
                        Name = nameWithoutExtension,
                        FilePath = shortcut,
                        IconLocation = shortcut
                    };
                    _cachedApps.Add(app);
                }
            }

            for (int i = 0; i < _cachedApps.Count;)
            {
                var cachedApp = _cachedApps[i];
                if (!File.Exists(cachedApp.FilePath))
                {
                    _uniqueAppNames.Remove(cachedApp.Name);
                    _cachedApps.Remove(cachedApp);
                    _lastPathWriteCache[Path.GetDirectoryName(cachedApp.FilePath)] = DateTime.MinValue;
                    continue;
                }

                i++;
                yield return cachedApp;
            }
        }

        private IEnumerable<string> FindApps(string path)
        {
            var extensions = new[]
            {
                "*.appref-ms",
                "*.lnk",
                "*.exe"
            };

            IEnumerable<string> SafeGetFiles(string ext)
            {
                try
                {
                    return Directory.GetFiles(path, ext, SearchOption.AllDirectories);
                }
                catch (UnauthorizedAccessException)
                {
                    return Array.Empty<string>();
                }
            }

            return extensions.SelectMany(SafeGetFiles);
        }

        private IEnumerable<string> GetPathsNeedUpdate()
        {
            foreach (var path in _paths.Where(Directory.Exists))
            {
                var currentLastWrite = Directory.GetLastWriteTime(path);
                if (_lastPathWriteCache.ContainsKey(path) && currentLastWrite <= _lastPathWriteCache[path])
                {
                    continue;
                }

                _lastPathWriteCache[path] = currentLastWrite;
                yield return path;
            }
        }
    }
}