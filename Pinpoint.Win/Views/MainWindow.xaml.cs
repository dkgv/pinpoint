using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Newtonsoft.Json;
using NHotkey;
using NHotkey.Wpf;
using Pinpoint.Core;
using Pinpoint.Core.Clipboard;
using Pinpoint.Plugin.Currency;
using Pinpoint.Plugin.Everything;
using Pinpoint.Plugin.AppSearch;
using Pinpoint.Plugin.Bangs;
using Pinpoint.Plugin.Bookmarks;
using Pinpoint.Plugin.Calculator;
using Pinpoint.Plugin.ClipboardManager;
using Pinpoint.Plugin.ClipboardUploader;
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
using Pinpoint.Plugin.Shortcuts;
using Pinpoint.Win.ViewModels;
using PinPoint.Plugin.Spotify;
using Pinpoint.Plugin.Text;
using Pinpoint.Plugin.Timezone;
using Pinpoint.Plugin.Translate;
using Pinpoint.Plugin.UrlLauncher;
using Pinpoint.Plugin.Weather;
using Pinpoint.Win.Annotations;
using WK.Libraries.SharpClipboardNS;
using Control = System.Windows.Forms.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using Pinpoint.Win.Utils;
using System.Windows.Media;
using Pinpoint.Plugin.Emoji;
using Pinpoint.Plugin.OpenAI;
using Pinpoint.Plugin;
using Pinpoint.Plugin.Windower;
using Pinpoint.Plugin.GuidGenerator;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts = new();
        private int _showingOptionsForIndex = -1;
        private double _leftOffsetRatio = 0, _topOffsetRatio = 0;
        private readonly WindowPositionHelper _windowPositionHelper = new();
        private bool _isClipboardManagerOpen;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = App.Current.MainViewModel;

            AppSettings.Load();

            RegisterHotkey(AppConstants.HotkeyToggleVisibilityId, App.Current.SettingsViewModel.HotkeyToggleVisibility, OnToggleVisibility);
            RegisterHotkey(AppConstants.HotkeyPasteId, App.Current.SettingsViewModel.HotkeyPasteClipboard, OnSystemClipboardPaste);

            Model.Clipboard.ClipboardChanged += ClipboardOnClipboardChanged;
            Model.PluginEngine.Listeners.Add(App.Current.SettingsWindow);
        }

        public MainViewModel Model => (MainViewModel)DataContext;

        private void ClipboardOnClipboardChanged([CanBeNull] object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (NativeProvider.GetForegroundWindow() == new WindowInteropHelper(this).Handle)
            {
                return;
            }

            IClipboardEntry entry = e.ContentType switch
            {
                SharpClipboard.ContentTypes.Text => new TextClipboardEntry
                {
                    Title = Model.Clipboard.ClipboardText.Trim().Replace("\n", "").Replace("\r", ""),
                    Content = Model.Clipboard.ClipboardText,
                    Copied = DateTime.Now
                },
                SharpClipboard.ContentTypes.Image => new ImageClipboardEntry
                {
                    Title = $"Image - Copied {DateTime.Now.ToShortDateString()}",
                    Content = Model.Clipboard.ClipboardImage,
                    Copied = DateTime.Now
                },
                _ => null
            };
            if (entry == null)
            {
                return;
            }

            ClipboardHelper.PrependToHistory(entry);
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
            if (AppSettings.TryGet(AppConstants.LocalPluginsDirectoryKey, out string localPluginsDirectory) && !string.IsNullOrEmpty(localPluginsDirectory))
            {
                await Model.PluginEngine.LoadLocalPlugins(localPluginsDirectory);
            }

            var addPluginTasks = new List<Task>
            {
                Model.PluginEngine.LoadPlugin(new EverythingPlugin()),
                Model.PluginEngine.LoadPlugin(new AppSearchPlugin()),
                Model.PluginEngine.LoadPlugin(new ControlPanelPlugin()),
                Model.PluginEngine.LoadPlugin(new CalculatorPlugin()),
                Model.PluginEngine.LoadPlugin(new CurrencyPlugin()),
                Model.PluginEngine.LoadPlugin(new MetricConverterPlugin()),
                Model.PluginEngine.LoadPlugin(new BangsPlugin()),
                Model.PluginEngine.LoadPlugin(new DictionaryPlugin()),
                Model.PluginEngine.LoadPlugin(new CommandLinePlugin()),
                Model.PluginEngine.LoadPlugin(new SpotifyPlugin()),
                Model.PluginEngine.LoadPlugin(new EncodeDecodePlugin()),
                Model.PluginEngine.LoadPlugin(new FinancePlugin()),
                Model.PluginEngine.LoadPlugin(new HackerNewsPlugin()),
                Model.PluginEngine.LoadPlugin(new BookmarksPlugin()),
                Model.PluginEngine.LoadPlugin(new NotesPlugin()),
                Model.PluginEngine.LoadPlugin(new ColorConverterPlugin()),
                Model.PluginEngine.LoadPlugin(new UrlLauncherPlugin()),
                Model.PluginEngine.LoadPlugin(new PasswordGeneratorPlugin()),
                Model.PluginEngine.LoadPlugin(new ClipboardManagerPlugin()),
                Model.PluginEngine.LoadPlugin(new WeatherPlugin()),
                Model.PluginEngine.LoadPlugin(new OperatingSystemPlugin()),
                Model.PluginEngine.LoadPlugin(new ProcessManagerPlugin()),
                Model.PluginEngine.LoadPlugin(new TextPlugin()),
                Model.PluginEngine.LoadPlugin(new ClipboardUploaderPlugin()),
                Model.PluginEngine.LoadPlugin(new TimezoneConverterPlugin()),
                Model.PluginEngine.LoadPlugin(new TranslatePlugin()),
                Model.PluginEngine.LoadPlugin(new ShortcutsPlugin()),
                Model.PluginEngine.LoadPlugin(new EmojiPlugin()),
                Model.PluginEngine.LoadPlugin(new OpenAIPlugin()),
                Model.PluginEngine.LoadPlugin(new WindowerPlugin()),
                Model.PluginEngine.LoadPlugin(new GuidGeneratorPlugin())
            };

            await Task.WhenAll(addPluginTasks);
        }

        public async void OnSystemClipboardPaste([CanBeNull] object sender, HotkeyEventArgs e)
        {
            if (ClipboardHelper.History.Count == 0)
            {
                return;
            }

            Model.Results.Clear();

            // Fetch clipboard results
            var clipboardManagerPlugin = Model.PluginEngine.GetPluginByType<ClipboardManagerPlugin>();
            var results = await clipboardManagerPlugin.ProcessQuery(null, _cts.Token).ToListAsync();
            AddResults(results);

            _isClipboardManagerOpen = true;
            TxtQuery.Focus();
            Model.Watermark = "Pinpoint (clipboard)";

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
                _isClipboardManagerOpen = false;
                Model.Watermark = "Pinpoint";
            }
            else
            {
                var windowOffset = _windowPositionHelper.CalculateWindowOffsetFromOffsetRatios(new WindowOffsetRatio(_leftOffsetRatio, _topOffsetRatio));

                Left = windowOffset.Left;
                Top = windowOffset.Top;

                Show();
                Activate();
                TxtQuery.Focus();
                TxtQuery.SelectAll();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MoveWindowToDefaultPosition();

            await LoadPlugins();

            Model.Watermark = "Pinpoint";
            TxtQuery.IsEnabled = true;
            TxtQuery.Focus();

            await CheckForUpdates();
        }

        private async Task CheckForUpdates()
        {
            var currentVersion = AppConstants.Version;
            var newestVersion = await HttpHelper.SendGet("https://usepinpoint.com/api/app_version", s =>
            {
                var json = JsonConvert.DeserializeObject<dynamic>(s);
                return json["version"].ToString();
            });
            if (currentVersion == newestVersion)
            {
                return;
            }

            var response = MessageBox.Show($"Pinpoint version {newestVersion} is available, you are using {currentVersion}. Do you want to update?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (response == MessageBoxResult.Yes)
            {
                ProcessHelper.OpenUrl("https://github.com/dkgv/pinpoint/releases");
            }
        }

        public void MoveWindowToDefaultPosition()
        {
            var defaultWindowPosition = ComputeDpiAwareWindowCenterPosition();
            Left = defaultWindowPosition.X;
            Top = defaultWindowPosition.Y;

            var defaultOffsetRatio = _windowPositionHelper.CalculateWindowOffsetRatioFromPoint(defaultWindowPosition);

            _leftOffsetRatio = defaultOffsetRatio.LeftOffsetRatio;
            _topOffsetRatio = defaultOffsetRatio.TopOffsetRatio;
        }

        private Point ComputeDpiAwareWindowCenterPosition(Screen screen = null)
        {
            screen ??= Screen.PrimaryScreen;

            var dpi = VisualTreeHelper.GetDpi(this);

            var screenWidth = screen.Bounds.Width;
            var screenHeight = screen.Bounds.Height;

            var windowWidth = (int)(Width * dpi.DpiScaleX);
            var windowHeight = (int)(Height * dpi.DpiScaleY);

            var x = (screenWidth / 2) - (windowWidth / 2);
            var y = (screenHeight / 5) + (windowHeight / 2);

            return new Point(x, y);
        }

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
            if (IsCtrlKeyDown())
            {
                // Check if CTRL+0-9 was pressed
                var isDigitPressed = (int)e.Key >= 35 && (int)e.Key <= 43;
                if (isDigitPressed)
                {
                    var resultIndex = (int)e.Key - 35;
                    LstResults.SelectedIndex = resultIndex;
                    TryOpenSelectedResult();
                }

                // Check if CTRL+, was pressed
                if (e.Key == Key.OemComma)
                {
                    ShowSettingsWindow();
                }


                if (e.Key == Key.L)
                {
                    TxtQuery.Focus();
                    TxtQuery.SelectAll();
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
                    TryAutoCompleteQuery();
                    break;

                case Key.Enter:
                    TryOpenSelectedResult();
                    break;

                case Key.Escape:
                    if (_showingOptionsForIndex != -1)
                    {
                        HideQueryResultOptions();
                    }
                    else if (_isClipboardManagerOpen)
                    {
                        _isClipboardManagerOpen = false;
                        Model.Results.Clear();
                        Model.CacheResults.Clear();
                        Model.Watermark = "Pinpoint";
                    }
                    else
                    {
                        Hide();
                    }
                    break;
            }
        }

        private void TxtQuery_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CancelRunningSearch();

            var query = TxtQuery.Text;

            if (_isClipboardManagerOpen)
            {
                FilterClipboardHistory(query);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    Model.Results.Clear();
                }

                if (!Model.PreviousQuery.Equals(query))
                {
                    _ = Dispatcher.Invoke(UpdateResults);
                }
            }

            Model.PreviousQuery = query;
        }

        private void FilterClipboardHistory(string query)
        {
            // Query was cleared
            if (string.IsNullOrEmpty(query) && Model.CacheResults.Any())
            {
                Model.Results.Clear();
                Model.Results.AddRange(Model.CacheResults);
                if (Model.Results.Any())
                {
                    LstResults.SelectedIndex = 0;
                }

                Model.CacheResults.Clear();
                return;
            }

            bool Predicate(AbstractQueryResult r) => !r.Title.ToLower().Contains(query?.ToLower() ?? string.Empty);

            // Query is shorter than previous (less strict)
            if (Model.PreviousQuery.Length > query?.Length)
            {
                Model.Results.Clear();
                Model.Results.AddRange(Model.CacheResults.Where(x => !Predicate(x)));
                return;
            }

            // First time filtering
            if (!Model.CacheResults.Any())
            {
                Model.CacheResults.AddRange(Model.Results);
            }

            Model.Results.RemoveWhere(Predicate);
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

            // Clear options of selected result
            Model.Results.Clear();

            // Add back actual search results
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

        private async Task UpdateResults()
        {
            var query = new Query(TxtQuery.Text.Trim());
            if (query.IsEmpty)
            {
                Model.Results.Clear();
                return;
            }

            var results = new List<AbstractQueryResult>();
            await foreach (var result in Model.PluginEngine.RunPlugins(_cts.Token, query)) {
                results.Add(result);
            }

            var shortcutKey = 0;
            foreach (var result in results)
            {
                if (Model.Results.Contains(result))
                {
                    Model.Results.Remove(result);
                }

                // If one of first 9 results, set keyboard shortcut for result
                if (Model.Results.TryAdd(result) && shortcutKey < 9)
                {
                    result.Shortcut = "CTRL+" + ++shortcutKey;
                }
            }

            Model.Results.RemoveWhere(r => !results.Contains(r));

            if (Model.Results.Count > 0 && LstResults.SelectedIndex == -1)
            {
                LstResults.SelectedIndex = 0;
            }
        }

        private void AddResults(List<AbstractQueryResult> results)
        {
            var shortcutKey = 0;
            foreach (var result in results)
            {
                // If one of first 9 results, set keyboard shortcut for result
                if (Model.Results.TryAdd(result) && shortcutKey < 9)
                {
                    result.Shortcut = "CTRL+" + ++shortcutKey;
                }
            }

            if (Model.Results.Count > 0 && LstResults.SelectedIndex == -1)
            {
                LstResults.SelectedIndex = 0;
            }
        }

        private void LstResults_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    TryOpenSelectedResult();
                    break;

                case Key.Up:
                    if (LstResults.SelectedIndex == 0)
                    {
                        // First item of list is already selected so focus query field
                        TxtQuery.Focus();
                        return;
                    }

                    LstResults.SelectedIndex = Math.Max(LstResults.SelectedIndex - 1, 0);
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

                case Key.Tab:
                    TryAutoCompleteQuery();
                    break;
            }
        }

        private static readonly Key[] Digits = { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9 };

        private static Key GetDigitDown() => Digits.FirstOrDefault(Keyboard.IsKeyDown);

        private static bool IsCtrlKeyDown() => (Control.ModifierKeys & Keys.Control) == Keys.Control;

        private static bool IsAltKeyDown() => Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.System);

        private void CancelRunningSearch()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
        }

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
                    selection.OnSelect();
                    Model.Results.Clear();
                    Model.Watermark = "Pinpoint";
                    _isClipboardManagerOpen = false;
                    break;

                default:
                    selection.OnSelect();
                    break;
            }

            _showingOptionsForIndex = -1;
            Hide();
        }

        private void TryAutoCompleteQuery()
        {
            if (Model.Results.Count <= 0 || LstResults.SelectedIndex <= -1)
            {
                return;
            }

            TxtQuery.Text = Model.Results[LstResults.SelectedIndex].Title;
            TxtQuery.CaretIndex = TxtQuery.Text.Length;
            TxtQuery.Focus();
        }

        private void LstResults_MouseUp(object sender, MouseButtonEventArgs e) => TryOpenSelectedResult();

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

                var windowOffsetRatio = _windowPositionHelper.CalculateWindowOffsetRatioFromWindowBounds(Left, Top);

                _leftOffsetRatio = windowOffsetRatio.LeftOffsetRatio;
                _topOffsetRatio = windowOffsetRatio.TopOffsetRatio;
            }
        }

        private void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && IsAltKeyDown())
            {
                TryOpenPrimaryOption();
                return;
            }

            if (e.Key is not (Key.LeftAlt or Key.RightAlt or Key.System))
            {
                return;
            }

            if (LstResults.SelectedIndex != -1)
            {
                ShowQueryResultOptions(LstResults.SelectedIndex);
                e.Handled = true;
            }
        }

        private void TxtQuery_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Model.Results.Count == 0)
            {
                return;
            }

            if (!TxtQuery.IsFocused)
            {
                TxtQuery.Focus();
                TxtQuery.CaretIndex = TxtQuery.Text.Length;
            }

            switch (e.Key)
            {
                case Key.Down:
                    LstResults.SelectedIndex++;
                    LstResults.ScrollIntoView(LstResults.SelectedItem);
                    break;

                case Key.Up:
                    if (LstResults.SelectedIndex > 0)
                    {
                        LstResults.SelectedIndex--;
                    }
                    LstResults.ScrollIntoView(LstResults.SelectedItem);
                    break;
            }
        }
    }
}
