using System.Collections.ObjectModel;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Core;

namespace Pinpoint.Win.Models
{
    internal class SettingsWindowModel : BaseControlModel
    {
        private HotkeyModel _hotkeyToggleVisibility = AppSettings.Contains("hotkey_toggle_visibility")
            ? new HotkeyModel(AppSettings.Get<HotkeyModel>("hotkey_toggle_visibility").Text)
            : new HotkeyModel("Alt + Space");

        private HotkeyModel _hotkeyPasteClipboard = AppSettings.Contains("hotkey_paste_clipboard")
            ? new HotkeyModel(AppSettings.Get<HotkeyModel>("hotkey_paste_clipboard").Text)
            : new HotkeyModel("Ctrl + Alt + V");

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
    }
}
