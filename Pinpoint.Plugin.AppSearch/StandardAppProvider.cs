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
            foreach (var path in Paths.Where(Directory.Exists))
            {
                var shortcuts = Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories);
                foreach (var shortcut in shortcuts)
                {
                    var name = Path.GetFileName(shortcut);
                    yield return new StandardApp
                    {
                        Name = name[..name.IndexOf(".")],
                        FilePath = shortcut,
                    };
                }
            }
        }
    }
}