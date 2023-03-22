using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace Pinpoint.Core
{
    public class ProcessHelper
    {
        private const string PowerShellConvertToJson = "ConvertTo-Json";

        public static T PowerShellToJson<T>(string args)
        {
            var suffix = $" | {PowerShellConvertToJson}";
            if (!args.EndsWith(suffix))
            {
                args += suffix;
            }

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

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var modUrl = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {modUrl}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        
        public static void OpenNotepad(string text)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, text);
            Process.Start("notepad.exe", tempFile);
        }
    }
}