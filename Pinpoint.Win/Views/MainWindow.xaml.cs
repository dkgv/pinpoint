using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using NHotkey;
using NHotkey.Wpf;
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
using Pinpoint.Plugin.ProcessManager;
using Pinpoint.Plugin.Reddit;
using Pinpoint.Win.ViewModels;
using PinPoint.Plugin.Spotify;
using Pinpoint.Plugin.UrlLauncher;
using Pinpoint.Plugin.Weather;
using Pinpoint.Win.Annotations;
using WK.Libraries.SharpClipboardNS;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace Pinpoint.Win.Views
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

        public MainWindow()
        {
            InitializeComponent();

            DataContext = App.Current.MainViewModel;

            RegisterHotkey(AppConstants.HotkeyToggleVisibilityId, App.Current.SettingsViewModel.HotkeyToggleVisibility, OnToggleVisibility);
            RegisterHotkey(AppConstants.HotkeyPasteId, App.Current.SettingsViewModel.HotkeyPasteClipboard, OnSystemClipboardPaste);

            Model.Clipboard.ClipboardChanged += ClipboardOnClipboardChanged;
            Model.PluginEngine.Listeners.Add(App.Current.SettingsWindow);
        }

        public MainViewModel Model => (MainViewModel) DataContext;

        private void ClipboardOnClipboardChanged([CanBeNull] object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            IClipboardEntry entry = e.ContentType switch
            {
                SharpClipboard.ContentTypes.Text => new TextClipboardEntry
                {
                    Title = Model.Clipboard.ClipboardText.Trim().Replace("\n", "").Replace("\r", ""), 
                    Content = Model.Clipboard.ClipboardText
                },
                SharpClipboard.ContentTypes.Image => new ImageClipboardEntry
                {
                    Title = $"Image - Copied {DateTime.Now.ToShortDateString()}", 
                    Content = Model.Clipboard.ClipboardImage
                },
                _ => null
            };

            if (entry != null)
            {
                Model.PluginEngine.PluginByType<ClipboardManagerPlugin>().ClipboardHistory.AddFirst(entry);
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
                var msg = $"Failed to register Pinpoint hotkey, {App.Current.SettingsViewModel.HotkeyToggleVisibility.Text} seems to already be bound. You can pick another one in settings.";
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPlugins()
        {
            var addPluginTasks = new List<Task>
            {
                Model.PluginEngine.AddPlugin(new EverythingPlugin()),
                Model.PluginEngine.AddPlugin(new AppSearchPlugin()),
                Model.PluginEngine.AddPlugin(new ControlPanelPlugin()),
                Model.PluginEngine.AddPlugin(new CalculatorPlugin()),
                Model.PluginEngine.AddPlugin(new CurrencyPlugin()),
                Model.PluginEngine.AddPlugin(new MetricConverterPlugin()),
                Model.PluginEngine.AddPlugin(new BangsPlugin()),
                Model.PluginEngine.AddPlugin(new DictionaryPlugin()),
                Model.PluginEngine.AddPlugin(new CommandLinePlugin()),
                Model.PluginEngine.AddPlugin(new SpotifyPlugin()),
                Model.PluginEngine.AddPlugin(new EncodeDecodePlugin()),
                Model.PluginEngine.AddPlugin(new FinancePlugin()),
                Model.PluginEngine.AddPlugin(new HackerNewsPlugin()),
                Model.PluginEngine.AddPlugin(new BookmarksPlugin()),
                Model.PluginEngine.AddPlugin(new RedditPlugin()),
                Model.PluginEngine.AddPlugin(new NotesPlugin()),
                Model.PluginEngine.AddPlugin(new ColorConverterPlugin()),
                Model.PluginEngine.AddPlugin(new UrlLauncherPlugin()),
                Model.PluginEngine.AddPlugin(new PasswordGeneratorPlugin()),
                Model.PluginEngine.AddPlugin(new ClipboardManagerPlugin()),
                Model.PluginEngine.AddPlugin(new WeatherPlugin()),
                Model.PluginEngine.AddPlugin(new OperatingSystemPlugin()),
                Model.PluginEngine.AddPlugin(new ProcessManagerPlugin())
            };

            await Task.WhenAll(addPluginTasks).ConfigureAwait(false);
            Dispatcher.Invoke(() =>
            {
                TxtQuery.Watermark = "Pinpoint";
                TxtQuery.IsEnabled = true;
                TxtQuery.Focus();
            });
        }

        public async void OnSystemClipboardPaste([CanBeNull] object sender, HotkeyEventArgs e)
        {
            var plugin = Model.PluginEngine.PluginByType<ClipboardManagerPlugin>();
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
            if (App.Current.SettingsWindow.Visibility == Visibility.Visible)
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

        protected override void OnClosing(CancelEventArgs e)
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
            // Check if CTRL+0-9 was pressed
            var isDigitPressed = (int)e.Key >= 35 && (int)e.Key <= 43;
            var resultIndex = (int)e.Key - 35;

            if (IsCtrlKeyDown())
            {
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
                var digit = GetDigitDown();
                if (digit != default)
                {
                    var number = int.Parse(digit.ToString().Replace("D", ""));
                    ShowQueryResultOptions(number - 1);
                }
                e.Handled = true;
            }

            switch (e.Key)
            {
                case Key.System:
                    e.Handled = true;
                    break;

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
                    else if (string.IsNullOrEmpty(TxtQuery.Text))
                    {
                        Hide();
                    }
                    break;

                case Key.Down:
                    if (Model.Results.Count > 0)
                    {
                        TxtQuery.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                    break;
            }
        }

        private void TxtQuery_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    if (Model.Results.Count > 0)
                    {
                        TxtQuery.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                    break;

                default:
                    if (!Model.PreviousQuery.Equals(TxtQuery.Text))
                    {
                        _ = Dispatcher.Invoke(async () => await UpdateResults());
                    }
                    break;
            }

            Model.PreviousQuery = TxtQuery.Text;
        }

        private void TxtQuery_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CancelRunningSearch();

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
                Model.CacheResults.Add(searchResult);
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
            foreach (var searchResult in Model.CacheResults)
            {
                Model.Results.TryAdd(searchResult);
            }
            Model.CacheResults.Clear();

            // Set selected index to owner of options
            LstResults.SelectedIndex = _showingOptionsForIndex;
            
            // Clear selection
            _showingOptionsForIndex = -1;
        }

        private async Task<bool> IsUserStillTyping()
        {
            var text = TxtQuery.Text;
            await Task.Delay(150);
            return !TxtQuery.Text.Equals(text);
        }

        private async Task UpdateResults()
        {
            if (await IsUserStillTyping())
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
            await AwaitAddEnumerable(Model.PluginEngine.Process(query, _cts.Token));

            Model.QueryHistory.Add(query);

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

        private void LstResults_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
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

        private void LstResults_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    TryOpenSelectedResult();
                    break;

                case Key.System:
                    if (IsAltKeyDown() && Keyboard.IsKeyDown(Key.Enter))
                    {
                        TryOpenPrimaryOption();
                    }
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
            }
        }

        private void AdjustQueryToHistory(bool older)
        {
            var next = older
                ? Model.QueryHistory.Current?.Next 
                : Model.QueryHistory.Current?.Previous;
            if (next != null)
            {
                Model.QueryHistory.Current = next;
                TxtQuery.Text = Model.QueryHistory.Current.Value.RawQuery;
                TxtQuery.CaretIndex = TxtQuery.Text.Length;
            }
        }

        private static readonly Key[] Digits = {Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9};

        private Key GetDigitDown() => Digits.FirstOrDefault(Keyboard.IsKeyDown);

        private bool IsCtrlKeyDown() => (Control.ModifierKeys & Keys.Control) == Keys.Control;

        private bool IsAltKeyDown() => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

        private void CancelRunningSearch() => _cts?.Cancel();

        private void TryOpenPrimaryOption()
        {
            if (LstResults.SelectedIndex == -1)
            {
                return;
            }

            var selection = Model.Results[LstResults.SelectedIndex];
            if (selection.OnPrimaryOptionSelect())
            {
                TxtQuery.Clear();
                Hide();
            }
        }

        private void TryOpenSelectedResult()
        {
            if (LstResults.SelectedIndex == -1)
            {
                return;
            }

            CancelRunningSearch();
            var selection = Model.Results[LstResults.SelectedIndex];

            TxtQuery.Clear();

            switch (selection)
            {
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
            App.Current.SettingsWindow.Show();
            Hide();
        }

        private void ItmExit_Click(object sender, RoutedEventArgs e) => App.Current.Shutdown();

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
