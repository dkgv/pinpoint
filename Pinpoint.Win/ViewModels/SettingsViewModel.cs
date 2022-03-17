using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pinpoint.Core;
using Pinpoint.Win.Views;

namespace Pinpoint.Win.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private List<PluginTabItem> _pluginTabItems = new List<PluginTabItem>();

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

        public ObservableCollection<IPlugin> Plugins { get; } = new();

        public List<PluginTabItem> PluginTabItems
        {
            get => _pluginTabItems;
            set => SetProperty(ref _pluginTabItems, value);
        }

        public string WindowTitle => "Pinpoint " + AppConstants.Version;
    }
}
