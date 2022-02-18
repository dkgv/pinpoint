using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarkdownSharp;
using Newtonsoft.Json;
using NHotkey.Wpf;
using Pinpoint.Core;
using Pinpoint.Win.ViewModels;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, IPluginListener<IPlugin, object>
    {
        public SettingsWindow()
        {
            InitializeComponent();

            DataContext = App.Current.SettingsViewModel;

            _ = Dispatcher.InvokeAsync(async () => await DownloadChangelog());
        }

        public SettingsViewModel Model => (SettingsViewModel) DataContext;

        private async Task DownloadChangelog()
        {
            try
            {
                using var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync("https://usepinpoint.com/api/github_changelog");
                Dispatcher.Invoke(() => UpdateLog.NavigateToString(html));
            }
            catch (HttpRequestException)
            {
            }
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            AppSettings.Put("theme", App.Current.MainViewModel.Theme);
            AppSettings.Save();

            foreach (var plugin in Model.Plugins)
            {
                plugin.Save();
            }

            Hide();
        }

        private void TxtHotkeyClipboardManager_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Clear modifier keys
            if (modifiers == ModifierKeys.None && (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                Model.HotkeyPasteClipboard = new HotkeyModel(Key.V, ModifierKeys.Control & ModifierKeys.Alt);
                return;
            }

            var invalidKeys = new[]
            {
                Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift, Key.LWin,
                Key.RWin, Key.Clear, Key.OemClear, Key.Apps
            };

            if (invalidKeys.Contains(key))
            {
                return;
            }

            // Save and set new hotkey
            Model.HotkeyPasteClipboard = new HotkeyModel(key, modifiers);
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyPasteId, Model.HotkeyPasteClipboard.Key, Model.HotkeyPasteClipboard.Modifiers, App.Current.MainWindow.OnSystemClipboardPaste);
            AppSettings.PutAndSave("hotkey_paste_clipboard", Model.HotkeyPasteClipboard);
        }

        private void TxtHotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            // Get modifiers and key data
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            // When Alt is pressed, SystemKey is used instead
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            // Clear modifier keys
            if (modifiers == ModifierKeys.None && (key == Key.Delete || key == Key.Back || key == Key.Escape))
            {
                Model.HotkeyToggleVisibility = new HotkeyModel(Key.Space, ModifierKeys.Alt);
                return;
            }

            var invalidKeys = new[]
            {
                Key.LeftCtrl, Key.RightCtrl, Key.LeftAlt, Key.RightAlt, Key.LeftShift, Key.RightShift, Key.LWin,
                Key.RWin, Key.Clear, Key.OemClear, Key.Apps
            };

            if (invalidKeys.Contains(key))
            {
                return;
            }

            // Save and set new hotkey
            Model.HotkeyToggleVisibility = new HotkeyModel(key, modifiers);
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyToggleVisibilityId, Model.HotkeyToggleVisibility.Key, Model.HotkeyToggleVisibility.Modifiers, App.Current.MainWindow.OnToggleVisibility);
            AppSettings.PutAndSave("hotkey_toggle_visibility", Model.HotkeyToggleVisibility);
        }

        public void PluginChange_Added(object sender, IPlugin plugin, object target)
        {
            Model.Plugins.Add(plugin);

            // Add to tab control
            var pluginTabItem = new PluginTabItem(plugin);
            if (plugin.Storage.UserSettings.Count == 0)
            {
                pluginTabItem.LblSettings.Visibility = pluginTabItem.PluginSettings.Visibility = Visibility.Hidden;
            }

            Model.PluginTabItems.Add(pluginTabItem);
            Model.PluginTabItems = Model.PluginTabItems.OrderBy(p => p.Model.Plugin.Meta.Name).ToList();
        }

        public void PluginChange_Removed(object sender, IPlugin plugin, object target) => Model.Plugins.Remove(plugin);

        private void CbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ThemeModel) CbTheme.SelectedItem;
            App.Current.MainWindow.Model.Theme = selected;
        }

        private void BtnReCenterWindow_OnClick(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.MoveWindowToDefaultPosition();
            Close();
            App.Current.MainWindow.Show();
        }
    }
}
