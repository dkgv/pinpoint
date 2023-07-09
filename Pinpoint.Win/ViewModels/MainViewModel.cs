using System.Runtime.InteropServices;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using WK.Libraries.SharpClipboardNS;

namespace Pinpoint.Win.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _watermark;
        private readonly PluginEngine _pluginEngine;

        public MainViewModel(PluginEngine pluginEngine)
        {
            _pluginEngine = pluginEngine;
            Watermark = "Loading plugins...";

            try
            {
                Clipboard = new SharpClipboard();
            }
            catch (ExternalException)
            {
            }
        }

        public ObservableUniqueCollection<AbstractQueryResult> Results { get; } = new();

        public ObservableUniqueCollection<AbstractQueryResult> CacheResults { get; set; } = new();

        public PluginEngine PluginEngine => _pluginEngine;

        public SharpClipboard Clipboard { get; }

        public string PreviousQuery { get; set; } = string.Empty;

        public string Watermark
        {
            get => _watermark;
            set => SetProperty(ref _watermark, value);
        }
    }
}
