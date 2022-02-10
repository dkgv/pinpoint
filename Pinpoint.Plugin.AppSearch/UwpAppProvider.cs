using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.AppSearch
{
    public class UwpAppProvider : IAppProvider
    {
        public IEnumerable<IApp> Provide()
        {
            const string args = @"Get-StartApps | Where-Object { $_.AppID.Contains('!') } | ConvertTo-Json";

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

            return JsonConvert.DeserializeObject<List<UwpApp>>(sb.ToString());
        }
    }
}