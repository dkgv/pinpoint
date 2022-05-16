using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pinpoint.Core;

namespace Pinpoint.Plugin.AppSearch
{
    public class UwpAppProvider : IAppProvider
    {
        private const string FindAppsCommand =
            @"Get-StartApps | Where-Object { $_.AppID.Contains('!') }";
        private const string FindIconsCommand =
            "Get-AppxPackage -PackageTypeFilter Main | ForEach-Object -Process { $manifest = Get-AppxPackageManifest $_.PackageFullName; $app = $manifest.Package.Applications.Application; $iconLocation = $app.VisualElements.Square44x44Logo | Select-Object -First 1; if ($iconLocation -eq $null) {$iconLocation = $app.VisualElements.Square30x30Logo}; if ($iconLocation -eq $null) {$iconLocation = $app.VisualElements.Logo}; return [PSCustomObject]@{PackageFamilyName = $_.PackageFamilyName; InstallLocation = $_.InstallLocation; IconLocation = $iconLocation} }";

        public IEnumerable<IApp> Provide()
        {
            var apps = ProcessHelper.PowerShellToJson<List<UwpApp>>(FindAppsCommand);
            FetchAndAddAppIcons(apps);
            return apps;
        }

        private static void FetchAndAddAppIcons(List<UwpApp> apps)
        {
            var appxPackages = ProcessHelper.PowerShellToJson<List<AppxPackage>>(FindIconsCommand);
            foreach (var app in apps)
            {
                // Look through only the apps that come from Get-StartApps
                var appFamilyName = app.FilePath[..app.FilePath.IndexOf("!")];
                var package = appxPackages.FirstOrDefault(package => appFamilyName == package.PackageFamilyName);
                if (package == default)
                {
                    continue;
                }

                // Separate the folder and the icon name
                var location = package.IconLocation as string;
                var idxOfLastDir = location.LastIndexOf("\\");

                if (idxOfLastDir >= 0)
                {
                    var iconName = location[(idxOfLastDir + 1)..];
                    var iconLocation = location[..idxOfLastDir];

                    iconLocation = package.InstallLocation + "\\" + iconLocation;
                    app.IconLocation = FindCorrectIconPath(iconLocation, iconName[..iconName.IndexOf(".")]);
                }
            }
        }

        private static string FindCorrectIconPath(string iconLocation, string name)
        {
            if (!Directory.Exists(iconLocation))
            {
                return "";
            }

            var files = Directory.GetFiles(iconLocation, $"{name}.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files.Where(f => f.Contains(".png")))
            {
                // Look for correct option of icon - scale between 150-800
                if (file.Contains(".scale-"))
                {
                    if (!Regex.IsMatch(file, @".*\.scale-(800|400|200|150)\.png"))
                    {
                        continue;
                    }

                    return file;
                }

                // If file is not named with scale or targetsize
                if (!file.Contains(".targetsize-"))
                {
                    return file;
                }

                // Look for correct option of icon - targetsize >= 48
                if (!Regex.IsMatch(file, @".*\.targetsize-((48)|(49)|(([5-9]\d|[1-9]\d{2,})))\.png"))
                {
                    continue;
                }

                return file;
            }

            return "";
        }
        record AppxPackage([JsonProperty("PackageFamilyName")] string PackageFamilyName, [JsonProperty("InstallLocation")] string InstallLocation, [JsonProperty("IconLocation")] object IconLocation);
    }
}