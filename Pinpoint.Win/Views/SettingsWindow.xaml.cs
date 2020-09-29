using Pinpoint.Core;
using System.ComponentModel;
using System.Windows;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            AppSettings.Save();
            base.OnClosing(e);
        }
    }
}
