using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pinpoint.Core;
using Pinpoint.Core.Results;
using WK.Libraries.SharpClipboardNS;

namespace Pinpoint.Win.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableUniqueCollection<AbstractQueryResult> Results { get; } = new ObservableUniqueCollection<AbstractQueryResult>();

        public ObservableUniqueCollection<AbstractQueryResult> CacheResults { get; set; } = new ObservableUniqueCollection<AbstractQueryResult>();

        public ThemeModel Theme { get; set; } = AppSettings.GetOrDefault("theme", ThemeModel.DarkTheme);

        public PluginEngine PluginEngine { get; set; } = new PluginEngine();

        public QueryHistory QueryHistory { get; } = new QueryHistory(10);

        public SharpClipboard Clipboard { get; } = new SharpClipboard();

        public string PreviousQuery { get; set; } = string.Empty;
    }
}
