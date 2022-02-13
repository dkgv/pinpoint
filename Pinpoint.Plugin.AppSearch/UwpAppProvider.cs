using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.AppSearch
{
    public class UwpAppProvider : IAppProvider
    {
        public IEnumerable<IApp> Provide()
        {
            const string args = @"Get-StartApps | Where-Object { $_.AppID.Contains('!') } | ConvertTo-Json";
            var apps = RunProcess<List<UwpApp>>(args);

            FetchAndSetAppIcons(apps);

            return apps;
        }

        private static T RunProcess<T>(string args)
        {
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

            return JsonConvert.DeserializeObject<T>(sb.ToString());
        }

        private static void FetchAndSetAppIcons(List<UwpApp> apps)
        {
            const string args =
                "Get-AppxPackage -PackageTypeFilter Main | ForEach-Object -Process { $manifest = Get-AppxPackageManifest $_.PackageFullName; $app = $manifest.Package.Applications.Application; $iconLocation = $app.VisualElements.Square44x44Logo | Select-Object -First 1; if ($iconLocation -eq $null) {$iconLocation = $app.VisualElements.Square30x30Logo}; if ($iconLocation -eq $null) {$iconLocation = $app.VisualElements.Logo}; return [PSCustomObject]@{PackageFamilyName = $_.PackageFamilyName; InstallLocation = $_.InstallLocation; IconLocation = $iconLocation} } | ConvertTo-Json";
            var appxPackages = RunProcess<List<AppxPackage>>(args);
            foreach (var app in apps)
            {
                var appFamilyName = app.FilePath[..app.FilePath.IndexOf("!")];
                foreach (var package in appxPackages.Where(package => appFamilyName == package.PackageFamilyName))
                {
                    var location = package.IconLocation as string;
                    var idxOfLastDir = location.LastIndexOf("\\");
                    var iconName = location[(idxOfLastDir + 1)..];
                    var iconLocation = location[..idxOfLastDir];

                    iconLocation = package.InstallLocation + "\\" + iconLocation;
                    app.IconLocation = FindCorrectIconPath(iconLocation, iconName.Split(".").First());
                }
            }
        }

        private static string FindCorrectIconPath(string iconLocation, string name)
        {
            if (!Directory.Exists(iconLocation)) return "";

            var files = Directory.GetFiles(iconLocation, $"{name}.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                if (!file.Contains(".png")) continue;

                if (file.Contains(".scale-"))
                {
                    if (Regex.IsMatch(file, @".*\.scale-(800|400|200|150)\.png"))
                    {
                        return file;
                    }
                    continue;
                }

                if (file.Contains(".targetsize-"))
                {
                    if (Regex.IsMatch(file, @".*\.targetsize-((48)|(49)|(([5-9]\d|[1-9]\d{2,})))\.png"))
                    {
                        return file;
                    }
                    continue;
                }

                return file;
            }
            return "";
        }
    }

    [Serializable]
    internal struct AppxPackage
    {
        [JsonProperty("PackageFamilyName")]
        public string PackageFamilyName { get; set; }

        [JsonProperty("InstallLocation")]
        public string InstallLocation { get; set; }

        [JsonProperty("IconLocation")]
        public object IconLocation { get; set; }
    }
}