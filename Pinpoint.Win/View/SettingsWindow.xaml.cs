using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using NHotkey.Wpf;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Core;
using Pinpoint.Win.Models;

namespace Pinpoint.Win.View
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, ISnippetListener, IPluginListener<IPlugin, object>
    {
        private readonly MainWindow _mainWindow;
        private readonly PluginEngine _pluginEngine;
        
        public SettingsWindow(MainWindow mainWindow, PluginEngine pluginEngine)
        {
            InitializeComponent();
            Model = new SettingsWindowModel();

            _mainWindow = mainWindow;
            _pluginEngine = pluginEngine;

            LblVersion.Content = "Version " + AppConstants.Version;
        }

        internal SettingsWindowModel Model
        {
            get => (SettingsWindowModel) DataContext;
            set => DataContext = value;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            AppSettings.Put("plugins", Model.Plugins.ToArray());
            AppSettings.Put("theme", _mainWindow.Model.Theme);
            AppSettings.Save();

            Hide();
        }

        private void BtnAddFileSnippet_Click(object sender, RoutedEventArgs e)
        {
            var fileOpener = new OpenFileDialog
            {
                Title = "Select Source(s)",
                CheckFileExists = true, 
                CheckPathExists = true,
                Multiselect = true
            };

            if (!fileOpener.ShowDialog().Value)
            {
                return;
            }

            foreach (var fileName in fileOpener.FileNames)
            {
                var fileSource = new FileSnippet(fileName);
                if (_pluginEngine.PluginByType<SnippetsPlugin>().AddSnippet(this, fileSource))
                {
                    Model.FileSnippets.Add(fileSource);
                }
            }
        }

        private void BtnAddManualSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var newSimpleSnippetWindow = new TextSnippetWindow(_pluginEngine);
            newSimpleSnippetWindow.Show();
            Hide();
        }

        private void BtnAddCustomSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var screenCaptureOverlay = new ScreenCaptureOverlayWindow(_pluginEngine);
            screenCaptureOverlay.Show();
            Hide();
        }

        private void LstFileSnippets_KeyDown(object sender, KeyEventArgs e)
        {
            HandleLstSnippetKeyDown(sender, Model.FileSnippets, e);
        }

        private void LstManualSnippets_OnKeyDown(object sender, KeyEventArgs e)
        {
            HandleLstSnippetKeyDown(sender, Model.ManualSnippets, e);
        }

        private void HandleLstSnippetKeyDown<T>(object sender, ObservableCollection<T> collection, KeyEventArgs e) where T : AbstractSnippet
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                RemoveSelectedSnippet(sender, collection);
            }
        }

        private void BtnRemoveFileSnippet_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedSnippet(LstFileSnippets, Model.FileSnippets);
        }

        private void BtnRemoveManualSnippet_OnClick(object sender, RoutedEventArgs e)
        {        
            RemoveSelectedSnippet(LstManualSnippets, Model.ManualSnippets);
        }

        private void RemoveSelectedSnippet<T>(object sender, ObservableCollection<T> collection) where T : AbstractSnippet
        {
            var lst = sender as ListBox;
            if (lst.SelectedIndex >= 0)
            {
                var index = lst.SelectedIndex;
                _pluginEngine.PluginByType<SnippetsPlugin>().RemoveSnippet(this, collection[index]);
                collection.RemoveAt(index);
            }
            else
            {
                MessageBox.Show("Please select a snippet to delete.", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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

        public void SnippetAdded(object sender, SnippetsPlugin plugin, AbstractSnippet target)
        {
            if (Equals(sender, this))
            {
                return;
            }

            switch (target)
            {
                case FileSnippet fileSnippet:
                    Model.FileSnippets.Add(fileSnippet);
                    break;

                case TextSnippet manualSnippet:
                    Model.ManualSnippets.Add(manualSnippet);
                    break;
            }
        }

        public void SnippetRemoved(object sender, SnippetsPlugin plugin, AbstractSnippet target)
        {
            if (Equals(sender, this))
            {
                return;
            }

            switch (target)
            {
                case FileSnippet fileSnippet:
                    Model.FileSnippets.Remove(fileSnippet);
                    break;

                case TextSnippet manualSnippet:
                    Model.ManualSnippets.Remove(manualSnippet);
                    break;
            }
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
            TbCtrl.Items.Insert(TbCtrl.Items.Count - 1, pluginTabItem);
        }

        public void PluginChange_Removed(object sender, IPlugin plugin, object target)
        {
            Model.Plugins.Remove(plugin);
        }

        private void CbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (ThemeModel) CbTheme.SelectedItem;
            _mainWindow.Model.Theme = selected;
        }

        private void LnkCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            ProcessHelper.OpenUrl(LnkCheckUpdate.NavigateUri.AbsoluteUri);
        }

        private void BtnReCenterWindow_OnClick(object sender, RoutedEventArgs e)
        {
            _mainWindow.MoveWindowToDefaultPosition();
            Close();
            _mainWindow.Show();
        }
    }
}
