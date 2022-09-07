using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pinpoint.Plugin.AppSearch
{
    public class StandardAppProvider : IAppProvider
    {
        private static readonly string[] Paths = {
            @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs",
            $@"C:\Users\{Environment.UserName}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs",
            $@"C:\Users\{Environment.UserName}\Desktop"
        };

        public IEnumerable<IApp> Provide()
        {
            var uniqueAppNames = new HashSet<string>();
            foreach (var path in Paths.Where(Directory.Exists))
            {
                foreach (var shortcut in GetShortcuts(path))
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

        private IEnumerable<string> GetShortcuts(string path)
        {
            var extensions = new[]
            {
                "*.appref-ms",
                "*.lnk"
            };
            
            return extensions.SelectMany(ext => Directory.GetFiles(path, ext, SearchOption.AllDirectories)); 
        }
    }
}