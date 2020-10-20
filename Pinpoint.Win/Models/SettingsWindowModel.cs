using System.Collections.ObjectModel;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Plugin;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BaseWindowModel
    {
        private HotkeyModel _hotkey = AppSettings.Contains("hotkey")
            ? new HotkeyModel(AppSettings.GetObject("hotkey")["Text"])
            : new HotkeyModel("Alt + Space");

        public ObservableCollection<ThemeModel> Themes { get; } = new ObservableCollection<ThemeModel>(new []
        {
            ThemeModel.LightTheme, 
            ThemeModel.DarkTheme, 
        });

        public HotkeyModel Hotkey
        {
            get => _hotkey;
            set => SetProperty(ref _hotkey, value);
        }
        
        public ObservableCollection<TextSnippet> ManualSnippets { get; } = new ObservableCollection<TextSnippet>();

        public ObservableCollection<FileSnippet> FileSnippets { get; } = new ObservableCollection<FileSnippet>();

        public ObservableCollection<IPlugin> Plugins { get; } = new ObservableCollection<IPlugin>();
    }
}
