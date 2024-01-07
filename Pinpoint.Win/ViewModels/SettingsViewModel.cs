using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pinpoint.Plugin;
using Pinpoint.Win.Utils;
using Pinpoint.Win.Views;

namespace Pinpoint.Win.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private ObservableCollection<PluginTabItem> _pluginTabItems = new();

        private HotkeyModel _hotkeyPasteClipboard = AppSettings.Contains("hotkey_paste_clipboard")
            ? new HotkeyModel(AppSettings.Get<HotkeyModel>("hotkey_paste_clipboard").Text)
            : new HotkeyModel("Ctrl + Alt + V");

        private HotkeyModel _hotkeyToggleVisibility = AppSettings.Contains("hotkey_toggle_visibility")
            ? new HotkeyModel(AppSettings.Get<HotkeyModel>("hotkey_toggle_visibility").Text)
            : new HotkeyModel("Alt + Space");

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

        public ObservableCollection<AbstractPlugin> Plugins { get; } = new();

        public ObservableCollection<PluginTabItem> PluginTabItems
        {
            get => _pluginTabItems;
            set => SetProperty(ref _pluginTabItems, value);
        }

        public string WindowTitle => "Pinpoint " + AppConstants.Version;

        private string _localPluginsDirectory = string.Empty;

        public string LocalPluginsDirectory
        {
            get => _localPluginsDirectory;
            set => SetProperty(ref _localPluginsDirectory, value);
        }
    }
}
