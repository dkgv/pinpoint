using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pinpoint.Plugin.AppSearch
{
    public class DirectoryAppProvider : IAppProvider
    {
        private readonly string[] _paths;

        public DirectoryAppProvider(string[] paths)
        {
            _paths = paths;
        }

        public IEnumerable<IApp> Provide()
        {
            var uniqueAppNames = new HashSet<string>();
            foreach (var path in _paths.Where(Directory.Exists))
            {
                foreach (var shortcut in FindApps(path))
                {
                    var nameWithoutExtension = Path.GetFileNameWithoutExtension(shortcut);
                    if (!uniqueAppNames.Add(nameWithoutExtension))
                    {
                        continue;
                    }

                    yield return new StandardApp
                    {
                        Name = nameWithoutExtension,
                        FilePath = shortcut,
                        IconLocation = shortcut
                    };
                }
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
    }
}