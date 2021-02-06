using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NHotkey;
using NHotkey.Wpf;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Core;
using Pinpoint.Plugin.Currency;
using Pinpoint.Plugin.Everything;
using Pinpoint.Core.MetricConverter;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.AppSearch;
using Pinpoint.Plugin.Bangs;
using Pinpoint.Plugin.Calculator;
using Pinpoint.Plugin.CommandLine;
using Pinpoint.Plugin.ControlPanel;
using Pinpoint.Plugin.Dictionary;
using Pinpoint.Plugin.EncodeDecode;
using Pinpoint.Plugin.Finance;
using Pinpoint.Plugin.HackerNews;
using Pinpoint.Plugin.MetricConverter;
using Pinpoint.Win.Models;
using PinPoint.Plugin.Spotify;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;
        private readonly List<AbstractQueryResult> _searchResults = new List<AbstractQueryResult>();
        private int _showingOptionsForIndex = -1;
        private readonly SettingsWindow _settingsWindow;
        private readonly PluginEngine _pluginEngine;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load old settings
            AppSettings.Load();

            Model = new MainWindowModel();
            
            _pluginEngine = new PluginEngine();
            _settingsWindow = new SettingsWindow(this, _pluginEngine);
            _pluginEngine.Listeners.Add(_settingsWindow);

            LoadPlugins();

            var hotkey = _settingsWindow.Model.Hotkey;
            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyIdentifier, hotkey.Key, hotkey.Modifiers, OnToggleVisibility);
        }

        private void LoadPlugins()
        {
            _pluginEngine.AddPlugin(new EverythingPlugin());
            _pluginEngine.AddPlugin(new AppSearchPlugin());
            _pluginEngine.AddPlugin(new ControlPanelPlugin());
            _pluginEngine.AddPlugin(new CalculatorPlugin());
            _pluginEngine.AddPlugin(new CurrencyPlugin());
            _pluginEngine.AddPlugin(new MetricConverterPlugin());
            _pluginEngine.AddPlugin(new BangsPlugin());
            _pluginEngine.AddPlugin(new DictionaryPlugin());
            _pluginEngine.AddPlugin(new CommandLinePlugin());
            _pluginEngine.AddPlugin(new SnippetsPlugin(_settingsWindow));
            _pluginEngine.AddPlugin(new SpotifyPlugin());
            _pluginEngine.AddPlugin(new EncodeDecodePlugin());
            _pluginEngine.AddPlugin(new FinancePlugin());
            _pluginEngine.AddPlugin(new HackerNewsPlugin());
        }

        internal MainWindowModel Model
        {
            get => (MainWindowModel)DataContext;
            set => DataContext = value;
        }

        public void OnToggleVisibility(object? sender, HotkeyEventArgs e)
        {
            if (_settingsWindow.Visibility == Visibility.Visible)
            {
                return;
            }

            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
            else
            {
                Show();
                Activate();
                TxtQuery.Focus();
                TxtQuery.SelectAll();
            }

            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto-focus query field
            TxtQuery.Clear();
            TxtQuery.Focus();

            // Locate window horizontal center near top of screen
            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 5;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            NotifyIcon.Dispose();
            base.OnClosing(e);
        }

        private void NotifyIcon_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
        }

        private void NotifyIcon_PreviewTrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
        }

        private void TxtQuery_KeyDown(object sender, KeyEventArgs e)
        {
            var isDigitPressed = (int)e.Key >= 35 && (int)e.Key <= 43;
            var index = (int)e.Key - 35;

            if (IsCtrlKeyDown())
            {
                // Check if CTRL + 0-9 was pressed
                if (isDigitPressed)
                {
                    LstResults.SelectedIndex = index;
                    OpenSelectedResult();
                }
            }
            else if (IsAltKeyDown())
            {
                if (isDigitPressed)
                {
                    ShowQueryResultOptions(index);
                }
                else if (LstResults.SelectedIndex != -1)
                {
                    ShowQueryResultOptions(LstResults.SelectedIndex);
                }

                e.Handled = true;
            }
        }

        private void ShowQueryResultOptions(int listIndex)
        {
            var item = LstResults.Items[listIndex] as AbstractQueryResult;
            if (!item.Options.Any() || _showingOptionsForIndex != -1)
            {
                return;
            }

            _showingOptionsForIndex = listIndex;

            // Cache actual search results
            foreach (var searchResult in Model.Results)
            {
                _searchResults.Add(searchResult);
            }

            // Remove search results
            Model.Results.Clear();

            // Add search result options
            foreach (var option in item.Options)
            {
                Model.Results.TryAdd(option);
            }

            TxtQuery.Focus();
            LstResults.SelectedIndex = 0;
        }

        private void HideQueryResultOptions()
        {
            if (_showingOptionsForIndex == -1)
            {
                return;
            }

            // Remove options
            Model.Results.Clear();

            // Add actual search results
            foreach (var searchResult in _searchResults)
            {
                Model.Results.TryAdd(searchResult);
            }
            _searchResults.Clear();

            // Set selected index to owner of options
            LstResults.SelectedIndex = _showingOptionsForIndex;
            
            // Clear selection
            _showingOptionsForIndex = -1;
        }

        private async void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsCtrlKeyDown())
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    if (LstResults.SelectedIndex >= 0)
                    {
                        OpenSelectedResult();
                        TxtQuery.Clear();
                    }
                    break;

                case Key.Down:
                    if (Model.Results.Count > 0)
                    {
                        TxtQuery.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                    break;

                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.Left:
                case Key.Right:
                case Key.Up:
                    break;

                case Key.Escape:
                    if (_showingOptionsForIndex != -1)
                    {
                        HideQueryResultOptions();
                    }
                    break;

                default:
                    if (_showingOptionsForIndex != -1 && e.Key == Key.System)
                    {
                        break;
                    }
                    await UpdateResults();
                    break;
            }
        }

        private void TxtQuery_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            StopSearching();

            if (string.IsNullOrWhiteSpace(TxtQuery.Text))
            {
                Model.Results.Clear();
            }
        }

        private async Task<bool> StillTyping()
        {
            var text = TxtQuery.Text;
            await Task.Delay(75);
            return !TxtQuery.Text.Equals(text);
        }

        private async Task UpdateResults()
        {
            if (await StillTyping())
            {
                return;
            }

            Model.Results.Clear();

            var query = new Query(TxtQuery.Text.Trim());

            if (query.IsEmpty)
            {
                return;
            }

            _cts = new CancellationTokenSource();

            var shortcutIndex = 0;

            await foreach(var result in _pluginEngine.Process(query, _cts.Token))
            {
                var didAdd = Model.Results.TryAdd(result);

                // If one of first 9 results, set keyboard shortcut for result
                if (didAdd && shortcutIndex < 9)
                {
                    result.Shortcut = "CTRL+" + ++shortcutIndex;
                }
            }

            if (Model.Results.Count > 0 && LstResults.SelectedIndex == -1)
            {
                LstResults.SelectedIndex = 0;
            }
        }

        private void LstResults_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OpenSelectedResult();
                    break;

                case Key.Up:
                    if (LstResults.SelectedIndex == 0)
                    {
                        // First item of list is already selected so focus query field
                        TxtQuery.Focus();
                    }
                    else
                    {
                        LstResults.SelectedIndex = Math.Max(LstResults.SelectedIndex - 1, 0);
                    }
                    break;

                case Key.Back:
                    if (TxtQuery.Text.Length > 0)
                    {
                        TxtQuery.Text = TxtQuery.Text[..^1];
                    }
                    TxtQuery.CaretIndex = TxtQuery.Text.Length;
                    TxtQuery.Focus();
                    break;

                case Key.Left:
                    TxtQuery.CaretIndex = TxtQuery.Text.Length - 1;
                    TxtQuery.Focus();
                    break;

                case Key.L:
                    if (IsCtrlKeyDown())
                    {
                        TxtQuery.Focus();
                        TxtQuery.SelectAll();
                    }
                    break;

                case Key.Escape:
                    if (_showingOptionsForIndex != -1)
                    {
                        HideQueryResultOptions();
                    }
                    break;

                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.System:
                    if (LstResults.SelectedIndex != -1)
                    {
                        ShowQueryResultOptions(LstResults.SelectedIndex);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private bool IsCtrlKeyDown() => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        private bool IsAltKeyDown() => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

        private void StopSearching() =>  _cts?.Cancel();

        private void OpenSelectedResult()
        {
            if (LstResults.SelectedItems.Count == 0)
            {
                return;
            }

            StopSearching();

            var selection = Model.Results[LstResults.SelectedIndex];

            if (selection is SnippetQueryResult result)
            {
                switch (result.Instance)
                {
                    case OcrTextSnippet s:
                        var ocrSnippetWindow = new OcrSnippetWindow(_pluginEngine, s);
                        ocrSnippetWindow.Show();
                        break;

                    case TextSnippet s:
                        var textSnippetWindow = new TextSnippetWindow(_pluginEngine, s);
                        textSnippetWindow.Show();
                        break;

                    case FileSnippet s:
                        Process.Start(s.FilePath);
                        break;
                }
            }
            else
            {
                selection.OnSelect();
            }

            Hide();
        }

        private void LstResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstResults.SelectedIndex >= 0)
            {
                OpenSelectedResult();
            }
        }

        private void ItmSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsWindow.Show();
            Hide();
        }

        private void ItmExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void ItmNewSimpleSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var newSimpleSnippetWindow = new TextSnippetWindow(_pluginEngine);
            newSimpleSnippetWindow.Show();
            Hide();
        }

        private void ItmNewCustomSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var screenCaptureOverlay = new ScreenCaptureOverlayWindow(_pluginEngine);
            screenCaptureOverlay.Show();
            Hide();
        }
    }
}