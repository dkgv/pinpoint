using System.Diagnostics;
using Newtonsoft.Json;

namespace Pinpoint.Plugin.AppSearch
{
    public class UwpApp : IApp
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("AppID")]
        public string FilePath { get; set; }

        public string IconLocation { get; set; } = "";

        public void Open()
        {
            Process.Start("explorer.exe", $"shell:AppsFolder\\{FilePath}");
        }

        public void OpenDirectory()
        {
        }
    }
}