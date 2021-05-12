using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Core;
using Pinpoint.Win.ViewControllers;

namespace Pinpoint.Win.ViewModels
{
    internal class SettingsWindowModel : BaseControlModel
    {
        private HotkeyModel _hotkeyToggleVisibility = AppSettings.Contains("hotkey_toggle_visibility")
            ? new HotkeyModel(AppSettings.Get<HotkeyModel>("hotkey_toggle_visibility").Text)
            : new HotkeyModel("Alt + Space");

        private HotkeyModel _hotkeyPasteClipboard = AppSettings.Contains("hotkey_paste_clipboard")
            ? new HotkeyModel(AppSettings.Get<HotkeyModel>("hotkey_paste_clipboard").Text)
            : new HotkeyModel("Ctrl + Alt + V");

        private List<PluginTabItem> _pluginTabItems = new List<PluginTabItem>();

        public ObservableCollection<ThemeModel> Themes { get; } = new ObservableCollection<ThemeModel>(new []
        {
            ThemeModel.LightTheme, 
            ThemeModel.DarkTheme, 
        });

        public HotkeyModel HotkeyToggleVisibility
        {
            get => _hotkeyToggleVisibility;
            set => SetProperty(ref _hotkeyToggleVisibility, value);
        }

        public HotkeyModel HotkeyPasteClipboard
        {
            get => _hotkeyPasteClipboard;
            set => SetProperty(ref _hotkeyPasteClipboard, value);
        }
        
        public ObservableCollection<TextSnippet> ManualSnippets { get; } = new ObservableCollection<TextSnippet>();

        public ObservableCollection<FileSnippet> FileSnippets { get; } = new ObservableCollection<FileSnippet>();

        public ObservableCollection<IPlugin> Plugins { get; } = new ObservableCollection<IPlugin>();

        public List<PluginTabItem> PluginTabItems
        {
            get => _pluginTabItems;
            set => SetProperty(ref _pluginTabItems, value);
        }
    }
}
