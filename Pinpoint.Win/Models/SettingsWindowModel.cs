using System.Collections.ObjectModel;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Plugin;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BaseWindowModel
    {
        private HotkeyModel _hotkey = new HotkeyModel(AppSettings.GetStrOrDefault("hotkey", "Alt + Space"));
        private ThemeModel _theme = AppSettings.GetStrOrDefault("theme", "dark").Equals("light") 
            ? ThemeModel.LightTheme 
            : ThemeModel.DarkTheme;

        public HotkeyModel Hotkey
        {
            get => _hotkey;
            set => SetProperty(ref _hotkey, value);
        }

        public ThemeModel Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }
        

        public ObservableCollection<TextSnippet> ManualSnippets { get; } = new ObservableCollection<TextSnippet>();

        public ObservableCollection<FileSnippet> FileSnippets { get; } = new ObservableCollection<FileSnippet>();

        public ObservableCollection<IPlugin> Plugins { get; } = new ObservableCollection<IPlugin>();
    }
}
