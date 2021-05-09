using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
using Pinpoint.Plugin.Snippets;
using Pinpoint.Core;
using Pinpoint.Plugin.Currency;
using Pinpoint.Plugin.Everything;
using Pinpoint.Core.Results;
using Pinpoint.Plugin.AppSearch;
using Pinpoint.Plugin.Bangs;
using Pinpoint.Plugin.Bookmarks;
using Pinpoint.Plugin.Calculator;
using Pinpoint.Plugin.ClipboardManager;
using Pinpoint.Plugin.ColorConverter;
using Pinpoint.Plugin.CommandLine;
using Pinpoint.Plugin.ControlPanel;
using Pinpoint.Plugin.Dictionary;
using Pinpoint.Plugin.EncodeDecode;
using Pinpoint.Plugin.Finance;
using Pinpoint.Plugin.HackerNews;
using Pinpoint.Plugin.MetricConverter;
using Pinpoint.Plugin.Notes;
using Pinpoint.Plugin.OperatingSystem;
using Pinpoint.Plugin.PasswordGenerator;
using Pinpoint.Plugin.Reddit;
using Pinpoint.Win.Models;
using PinPoint.Plugin.Spotify;
using Pinpoint.Plugin.UrlLauncher;
using Pinpoint.Plugin.Weather;
using Pinpoint.Win.Annotations;
using WK.Libraries.SharpClipboardNS;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace Pinpoint.Win.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;
        private int _showingOptionsForIndex = -1;
        private double _offsetFromDefaultX = 0, _offsetFromDefaultY = 0;
        private Point _defaultWindowPosition;
        private bool _wasModifierKeyDown = false;
        private string _prevQuery = string.Empty;

        private readonly SettingsWindow _settingsWindow;
        private readonly PluginEngine _pluginEngine;
        private readonly QueryHistory _queryHistory;
        private readonly List<AbstractQueryResult> _searchResults = new List<AbstractQueryResult>();
        private readonly SharpClipboard _clipboard = new SharpClipboard();
        
        public MainWindow()
        {
            InitializeComponent();

            Model = new MainWindowModel();
            
            _pluginEngine = new PluginEngine();
            _queryHistory = new QueryHistory(10);
            _settingsWindow = new SettingsWindow(this, _pluginEngine);

            _pluginEngine.Listeners.Add(_settingsWindow);

            RegisterHotkey(AppConstants.HotkeyToggleVisibilityId, _settingsWindow.Model.HotkeyToggleVisibility, OnToggleVisibility);
            RegisterHotkey(AppConstants.HotkeyPasteId, _settingsWindow.Model.HotkeyPasteClipboard, OnSystemClipboardPaste);

            _clipboard.ClipboardChanged += ClipboardOnClipboardChanged;
        }

        private void ClipboardOnClipboardChanged([CanBeNull] object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            IClipboardEntry entry = e.ContentType switch
            {
                SharpClipboard.ContentTypes.Text => new TextClipboardEntry
                {
                    Title = _clipboard.ClipboardText.Trim().Replace("\n", "").Replace("\r", ""), 
                    Content = _clipboard.ClipboardText
                },
                SharpClipboard.ContentTypes.Image => new ImageClipboardEntry
                {
                    Title = $"Image - Copied {DateTime.Now.ToShortDateString()}", 
                    Content = _clipboard.ClipboardImage
                },
                _ => null
            };

            if (entry != null)
            {
                _pluginEngine.PluginByType<ClipboardManagerPlugin>().ClipboardHistory.AddFirst(entry);
            }
        }

        private void RegisterHotkey(string identifier, HotkeyModel hotkey, EventHandler<HotkeyEventArgs> handler)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(identifier, hotkey.Key, hotkey.Modifiers, handler);
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                var msg = $"Failed to register Pinpoint hotkey, {_settingsWindow.Model.HotkeyToggleVisibility.Text} seems to already be bound. You can pick another one in settings.";
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPlugins()
        {
            var addPluginTasks = new List<Task>
            {
                _pluginEngine.AddPlugin(new EverythingPlugin()),
                _pluginEngine.AddPlugin(new AppSearchPlugin()),
                _pluginEngine.AddPlugin(new ControlPanelPlugin()),
                _pluginEngine.AddPlugin(new CalculatorPlugin()),
                _pluginEngine.AddPlugin(new CurrencyPlugin()),
                _pluginEngine.AddPlugin(new MetricConverterPlugin()),
                _pluginEngine.AddPlugin(new BangsPlugin()),
                _pluginEngine.AddPlugin(new DictionaryPlugin()),
                _pluginEngine.AddPlugin(new CommandLinePlugin()),
                _pluginEngine.AddPlugin(new SnippetsPlugin(_settingsWindow)),
                _pluginEngine.AddPlugin(new SpotifyPlugin()),
                _pluginEngine.AddPlugin(new EncodeDecodePlugin()),
                _pluginEngine.AddPlugin(new FinancePlugin()),
                _pluginEngine.AddPlugin(new HackerNewsPlugin()),
                _pluginEngine.AddPlugin(new BookmarksPlugin()),
                _pluginEngine.AddPlugin(new RedditPlugin()),
                _pluginEngine.AddPlugin(new NotesPlugin()),
                _pluginEngine.AddPlugin(new ColorConverterPlugin()),
                _pluginEngine.AddPlugin(new UrlLauncherPlugin()),
                _pluginEngine.AddPlugin(new PasswordGeneratorPlugin()),
                _pluginEngine.AddPlugin(new ClipboardManagerPlugin()),
                _pluginEngine.AddPlugin(new WeatherPlugin()),
                _pluginEngine.AddPlugin(new OperatingSystemPlugin()),
            };

            await Task.WhenAll(addPluginTasks).ConfigureAwait(false);
        }

        internal MainWindowModel Model
        {
            get => (MainWindowModel)DataContext;
            set => DataContext = value;
        }

        public async void OnSystemClipboardPaste([CanBeNull] object sender, HotkeyEventArgs e)
        {
            var plugin = _pluginEngine.PluginByType<ClipboardManagerPlugin>();
            if (plugin.ClipboardHistory.Count == 0)
            {
                return;
            }

            // Remove old results and add clipboard history content
            Model.Results.Clear();
            await AwaitAddEnumerable(plugin.Process(null));

            if (Visibility != Visibility.Visible)
            {
                OnToggleVisibility(sender, e);
            }
        }

        public void OnToggleVisibility([CanBeNull] object sender, HotkeyEventArgs e)
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
                var screen = Screen.FromPoint(System.Windows.Forms.Cursor.Position);
                Left = (screen.Bounds.Left + screen.Bounds.Width / 2 - Width / 2) + _offsetFromDefaultX;
                Top = (screen.Bounds.Top + screen.Bounds.Height / 5) + _offsetFromDefaultY;

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

            _defaultWindowPosition = ComputeDefaultWindowPosition();
            MoveWindowToDefaultPosition();

            AppSettings.Load();
            LoadPlugins();
        }

        public void MoveWindowToDefaultPosition()
        {
            // Locate window horizontal center near top of screen
            Left = _defaultWindowPosition.X;
            Top = _defaultWindowPosition.Y;
            _offsetFromDefaultX = _offsetFromDefaultY = 0;
        }

        private Point ComputeDefaultWindowPosition() => new Point(SystemParameters.PrimaryScreenWidth / 2 - Width / 2, SystemParameters.PrimaryScreenHeight / 5);

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

        private void TxtQuery_OnKeyDown(object sender, KeyEventArgs e)
        {
            var isDigitPressed = (int)e.Key >= 35 && (int)e.Key <= 43;
            var resultIndex = (int)e.Key - 35;

            if (IsCtrlKeyDown())
            {
                // Check if CTRL+0-9 was pressed
                if (isDigitPressed)
                {
                    LstResults.SelectedIndex = resultIndex;
                    TryOpenSelectedResult();
                }

                // Check if CTRL+, was pressed
                if (e.Key == Key.OemComma)
                {
                    ShowSettingsWindow();
                }

                // Check if CTRL+UP or CTRL+DOWN was pressed
                if (e.Key == Key.Up)
                {
                    AdjustQueryToHistory(true);
                }
                else if (e.Key == Key.Down)
                {
                    AdjustQueryToHistory(false);
                }
            }
            else if (IsAltKeyDown())
            {
                if (isDigitPressed)
                {
                    ShowQueryResultOptions(resultIndex);
                }
                else if (LstResults.SelectedIndex != -1)
                {
                    ShowQueryResultOptions(LstResults.SelectedIndex);
                }

                e.Handled = true;
            }

            _wasModifierKeyDown = Control.ModifierKeys != Keys.None;

            switch (e.Key)
            {
                case Key.Tab:
                    if (Model.Results.Count > 0)
                    {
                        TxtQuery.Text = Model.Results.First().Title;
                        TxtQuery.CaretIndex = TxtQuery.Text.Length;
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    TryOpenSelectedResult();
                    break;

                case Key.Escape:
                    if (_showingOptionsForIndex != -1)
                    {
                        HideQueryResultOptions();
                    }
                    break;
            }
        }

        private void TxtQuery_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_wasModifierKeyDown && _prevQuery.Equals(TxtQuery.Text) || _showingOptionsForIndex != -1 && e.Key == Key.System)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Down:
                    if (Model.Results.Count > 0)
                    {
                        TxtQuery.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                    break;

                default:
                    if (!_prevQuery.Equals(TxtQuery.Text))
                    {
                        _ = Dispatcher.Invoke(async () => await UpdateResults());
                    }
                    break;
            }

            _prevQuery = TxtQuery.Text;
        }

        private void TxtQuery_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            StopSearching();

            if (string.IsNullOrWhiteSpace(TxtQuery.Text))
            {
                Model.Results.Clear();
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

        private async Task<bool> StillTyping()
        {
            var text = TxtQuery.Text;
            await Task.Delay(150);
            return !TxtQuery.Text.Equals(text);
        }

        private async Task UpdateResults()
        {
            if (await StillTyping())
            {
                return;
            }

            var query = new Query(TxtQuery.Text.Trim());
            if (query.IsEmpty)
            {
                return;
            }

            Model.Results.Clear();

            _cts = new CancellationTokenSource();
            await AwaitAddEnumerable(_pluginEngine.Process(query, _cts.Token));

            _queryHistory.Add(query);

            if (Model.Results.Count > 0 && LstResults.SelectedIndex == -1)
            {
                LstResults.SelectedIndex = 0;
            }
        }

        private async Task AwaitAddEnumerable(IAsyncEnumerable<AbstractQueryResult> enumerable)
        {
            var shortcutIndex = 0;

            await foreach (var result in enumerable)
            {
                var didAdd = Model.Results.TryAdd(result);

                // If one of first 9 results, set keyboard shortcut for result
                if (didAdd && shortcutIndex < 9)
                {
                    result.Shortcut = "CTRL+" + ++shortcutIndex;
                }
            }
        }

        private void LstResults_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    TryOpenSelectedResult();
                    break;

                case Key.Down:
                    if (IsCtrlKeyDown())
                    {
                        AdjustQueryToHistory(false);
                    }
                    break;

                case Key.Up:
                    if (IsCtrlKeyDown())
                    {
                        AdjustQueryToHistory(true);
                    }
                    else
                    {
                        if (LstResults.SelectedIndex == 0)
                        {
                            // First item of list is already selected so focus query field
                            TxtQuery.Focus();
                        }
                        else
                        {
                            LstResults.SelectedIndex = Math.Max(LstResults.SelectedIndex - 1, 0);
                        }
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

                case Key.OemComma:
                    if (IsCtrlKeyDown())
                    {
                        ShowSettingsWindow();
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

        private void AdjustQueryToHistory(bool older)
        {
            var next = older ? _queryHistory.Current?.Next : _queryHistory.Current?.Previous;
            if (next != null)
            {
                _queryHistory.Current = next;
                TxtQuery.Text = _queryHistory.Current.Value.RawQuery;
                TxtQuery.CaretIndex = TxtQuery.Text.Length;
            }
        }

        private bool IsCtrlKeyDown() => (Control.ModifierKeys & Keys.Control) == Keys.Control;

        private bool IsAltKeyDown() => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

        private void StopSearching() => _cts?.Cancel();

        private void TryOpenSelectedResult()
        {
            if (LstResults.SelectedIndex == -1)
            {
                return;
            }

            StopSearching();
            var selection = Model.Results[LstResults.SelectedIndex];
            TxtQuery.Clear();
            
            switch (selection)
            {
                case SnippetQueryResult result:
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
                    break;

                case ClipboardResult _:
                    Model.Results.Clear();
                    selection.OnSelect();
                    break;

                default:
                    selection.OnSelect();
                    break;
            }

            _showingOptionsForIndex = -1;
            Hide();
        }

        private void LstResults_MouseDoubleClick(object sender, MouseButtonEventArgs e) => TryOpenSelectedResult();

        private void ItmSettings_Click(object sender, RoutedEventArgs e) => ShowSettingsWindow();

        private void ShowSettingsWindow()
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

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

                _offsetFromDefaultX = Left - _defaultWindowPosition.X;
                _offsetFromDefaultY = Top - _defaultWindowPosition.Y;
            }
        }
    }
}
