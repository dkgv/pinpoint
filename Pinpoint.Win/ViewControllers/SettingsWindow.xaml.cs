using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Win32;
using Newtonsoft.Json;
using NHotkey.Wpf;
using Pinpoint.Core;
using Pinpoint.Win.ViewModels;

namespace Pinpoint.Win.ViewControllers
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, IPluginListener<IPlugin, object>
    {
        private readonly MainWindow _mainWindow;
        private readonly PluginEngine _pluginEngine;
        
        public SettingsWindow(MainWindow mainWindow, PluginEngine pluginEngine)
        {
            InitializeComponent();
            Model = new SettingsWindowModel();

            _mainWindow = mainWindow;
            _pluginEngine = pluginEngine;

            _ = Dispatcher.InvokeAsync(async () => await PopulateUpdateLog());
        }

        internal SettingsWindowModel Model
        {
            get => (SettingsWindowModel) DataContext;
            set => DataContext = value;
        }

        private async Task PopulateUpdateLog()
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Pinpoint");
                var json = await httpClient.GetStringAsync("https://api.github.com/repos/dkgv/pinpoint/releases");
                var releases = JsonConvert.DeserializeObject<List<Release>>(json);
                var sb = new StringBuilder();
                sb.Append("# Changelog\n");
                foreach (var release in releases)
                {
                    sb.Append("## [Pinpoint ").Append(release.TagName).Append("](").Append(release.Assets[0].BrowserDownloadUrl).Append(")\n");
                    sb.Append("*Released on ").Append(DateTime.Parse(release.PublishedAt, new DateTimeFormatInfo())).Append(".*\n\n");
                    sb.Append(release.Body.Replace("\r\n", "<br>")).Append("\n\n");
                }

                var md = new Markdown();
                var html = md.Transform(sb.ToString());
                Dispatcher.Invoke(() => UpdateLog.NavigateToString(html));
            }
            catch (HttpRequestException)
            {
            }
        }

        private class Release
        {
            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("published_at")]
            public string PublishedAt { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("assets")]
            public Asset[] Assets { get; set; }

            public class Asset
            {
                [JsonProperty("browser_download_url")]
                public string BrowserDownloadUrl { get; set; }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            AppSettings.Put("plugins", Model.Plugins.ToArray());
            AppSettings.Put("theme", _mainWindow.Model.Theme);
            AppSettings.Save();

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
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyPasteId, Model.HotkeyPasteClipboard.Key, Model.HotkeyPasteClipboard.Modifiers, _mainWindow.OnSystemClipboardPaste);
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
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyToggleVisibilityId, Model.HotkeyToggleVisibility.Key, Model.HotkeyToggleVisibility.Modifiers, _mainWindow.OnToggleVisibility);
            AppSettings.PutAndSave("hotkey_toggle_visibility", Model.HotkeyToggleVisibility);
        }

        public void PluginChange_Added(object sender, IPlugin plugin, object target)
        {
            Model.Plugins.Add(plugin);

            // Add to tab control
            var pluginTabItem = new PluginTabItem(plugin);
            if (plugin.UserSettings.Count == 0)
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
            _mainWindow.Model.Theme = selected;
        }

        private void BtnReCenterWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.MoveWindowToDefaultPosition();
            Close();
            _mainWindow.Show();
        }
    }
}
