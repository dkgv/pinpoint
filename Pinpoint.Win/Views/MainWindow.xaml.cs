using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FontAwesome.WPF;
using Hardcodet.Wpf.TaskbarNotification;
using Pinpoint.Core;
using Xceed.Wpf.Toolkit;
using Color = System.Windows.Media.Color;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtQuery.Clear();
            TxtQuery.Focus();
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Click");
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Show();
            Close();
        }

        private void BtnSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            SetSettingsColor(sender, 216, 216, 216);
        }

        private void BtnSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            SetSettingsColor(sender, 87, 87, 87);
        }

        private void SetSettingsColor(object sender, byte r, byte g, byte b)
        {
            var source = (IconButton)sender;
            source.Icon = new ImageAwesome
            {
                Icon = FontAwesomeIcon.Cogs,
                Height = source.Icon.Height,
                Foreground = new SolidColorBrush(Color.FromRgb(r, g, b)),
            };
        }

        private async void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
            }
            else if (e.Key == Key.Down)
            {
                
            } 
            else if (e.Key == Key.Up)
            {
                
            }
            else
            { 
                await UpdateResults();
            }
        }

        private async Task UpdateResults()
        {
            var query = new Query(TxtQuery.Text);

        }
    }
}